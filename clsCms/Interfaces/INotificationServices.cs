
namespace clsCms.Services
{
    /// <summary>
    /// Provides methods for sending notifications, such as email.
    /// </summary>
    public interface INotificationServices
    {
        /// <summary>
        /// Sends an email using the configured SendGrid service and returns a boolean indicating success or failure.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body content of the email.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success (true) or failure (false).</returns>
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
    }
}