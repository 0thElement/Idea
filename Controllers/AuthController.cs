using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Idea.Data;
using Idea.Models;
using Idea.Services;

namespace Idea.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthController(DataContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _config = configuration;
            _emailService = emailService;
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using (var hmac = new HMACSHA512())
            {
                salt = hmac.Key;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool ValidPassword(UserLogin login, User user)
        {
            byte[] hash = user.PasswordHash;
            byte[] salt = user.PasswordSalt;
            using (var hmac = new HMACSHA512(salt))
            {
                byte[] loginHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
                return loginHash.SequenceEqual(hash);
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegister register)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'DataContext.Users'  is null.");
            }
            CreatePasswordHash(register.Password, out byte[] hash, out byte[] salt);

            Guid guid = Guid.NewGuid();

            var user = new User
            {
                Email = register.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Verified = false,
                VerificationToken = guid.ToString()
            };

            string? url = Url.Action(null, "confirm", new { token = user.VerificationToken }, protocol: Request.Scheme);
            if (url == null) return Problem("Could not generate verification URL");

            await _emailService.SendEmail(new EmailDto{
                From = _config["SendGrid:Email"],
                To = register.Email,
                Subject = "Complete your registration to Idea-Web",
                Body = $"Please use this link to complete your registration: {url}"
            });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Email }, new {
                email = user.Email,
                verified = user.Verified
            });
        }

        [HttpPost("verify")]
        public async Task<ActionResult> VerifyUser(string token)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'DataContext.Users'  is null.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null) return NotFound();

            user.Verified = true;
            user.VerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok("User verified");
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLogin login)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(login.Email);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.Verified) return Unauthorized("User not verified");
            if (!ValidPassword(login, user)) return Unauthorized("Wrong username or password");

            var claims = new [] {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken
            (
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                notBefore: DateTime.UtcNow,
                signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials
                (
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha512
                )
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(tokenString);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddDays(1);

            string? url = Url.Action(null, "reset", new { token = user.PasswordResetToken }, protocol: Request.Scheme);
            if (url == null) return Problem("Could not generate reset password URL");

            await _emailService.SendEmail(new EmailDto{
                From = _config["SendGrid:Email"],
                To = email,
                Subject = "Reset your Idea-Web account's password",
                Body = $"Please use this link to reset your account's password: {url}"
            });

            await _context.SaveChangesAsync();

            return Ok("Reset token generated");
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(UserResetPassword reset)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == reset.PasswordResetToken);
            Console.WriteLine(reset.PasswordResetToken);

            if (user == null) return NotFound();
            if (user.PasswordResetTokenExpires < DateTime.UtcNow) return BadRequest("Token expired");

            CreatePasswordHash(reset.Password, out byte[] hash, out byte[] salt);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Password reset");
        }
    }
}
