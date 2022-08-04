using Idea.Models;

namespace Idea.Services;

public interface IEmailService
{
    Task<bool> SendEmail(EmailDto email);
}
