using Idea.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
namespace Idea.Services;

public class EmailService : IEmailService
{
    private SendGridClient client;
    public EmailService(IConfiguration config)
    {
        string apiKey = config["SendGrid:ApiKey"];
        client = new SendGridClient(apiKey);
    }

    public async Task<bool> SendEmail(EmailDto email)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(email.From),
            Subject = email.Subject,
            HtmlContent = email.Body
        };

        msg.AddTo(new EmailAddress(email.To));
        return (await client.SendEmailAsync(msg)).IsSuccessStatusCode;
    }
}