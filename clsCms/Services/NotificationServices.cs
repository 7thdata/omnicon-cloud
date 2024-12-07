
using System.Net;
using Azure.Core;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace clsCms.Services
{

    public class NotificationServices : INotificationServices
    {
        private readonly AppConfigModel _appConfig;

        public NotificationServices(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Sends an email using the SendGrid service and returns a boolean indicating success or failure.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body content of the email.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success (true) or failure (false).</returns>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            var sendGridApiKey = _appConfig.SendGrid.SendGridApiKey;
            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress("talk2@omnicon.cloud", "Customer Support of OMNICON.Cloud");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            var response = await client.SendEmailAsync(msg);

            // Return true if the email was sent successfully, otherwise return false
            return response.StatusCode ==  HttpStatusCode.Accepted;
        }
    }
}