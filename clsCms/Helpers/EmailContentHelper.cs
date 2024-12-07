namespace clsCms.Helpers
{
    /// <summary>
    /// Provides methods to generate email content for various ASP.NET Core Identity events.
    /// </summary>
    public static class EmailContentHelper
    {
        /// <summary>
        /// Generates the email content for user registration.
        /// </summary>
        /// <param name="userName">The name of the registered user.</param>
        /// <param name="confirmationLink">The link to confirm the registration.</param>
        /// <returns>A string containing the email content.</returns>
        public static string GenerateRegistrationEmail(string userName, string confirmationLink)
        {
            return $@"
                <h1>Welcome, {userName}!</h1>
                <p>Thank you for registering. Please confirm your email by clicking the link below:</p>
                <a href='{confirmationLink}'>Confirm Email</a>
                <p>If you did not register for this account, please ignore this email.</p>
            ";
        }

        /// <summary>
        /// Generates the email content for registration completion.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <returns>A string containing the email content.</returns>
        public static string GenerateRegistrationCompleteEmail(string userName)
        {
            return $@"
                <h1>Registration Complete, {userName}!</h1>
                <p>Your registration is now complete. You can now log in to your account.</p>
            ";
        }

        /// <summary>
        /// Generates the email content for password reset request.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="resetLink">The link to reset the password.</param>
        /// <returns>A string containing the email content.</returns>
        public static string GeneratePasswordResetEmail(string userName, string resetLink)
        {
            return $@"
                <h1>Password Reset Request</h1>
                <p>Hi {userName},</p>
                <p>You requested to reset your password. Click the link below to set a new password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>If you did not request a password reset, please ignore this email.</p>
            ";
        }

        /// <summary>
        /// Generates the email content for password reset completion.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <returns>A string containing the email content.</returns>
        public static string GeneratePasswordResetCompleteEmail(string userName)
        {
            return $@"
                <h1>Password Reset Successfully</h1>
                <p>Hi {userName},</p>
                <p>Your password has been successfully reset. You can now log in with your new password.</p>
            ";
        }

        /// <summary>
        /// Generates the email content for a generic notification.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="message">The notification message.</param>
        /// <returns>A string containing the email content.</returns>
        public static string GenerateGenericNotificationEmail(string userName, string message)
        {
            return $@"
                <h1>Hello, {userName}!</h1>
                <p>{message}</p>
            ";
        }
    }
}
