namespace System
{
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using MSharp.Framework;
    using MSharp.Framework.Services;
    using Web;

    partial class MSharpExtensionsWeb
    {
        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static IEmailQueueItem SendAsNotification(this Exception error)
        {
            return SendAsNotification(error, Config.Get("Error.Notification.Receiver"));
        }

        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static IEmailQueueItem SendAsNotification(this Exception error, string toNotify)
        {
            if (toNotify.IsEmpty()) return null;
            var email = EmailService.EmailQueueItemFactory();
            email.To = toNotify;
            email.Subject = "Error In Application";
            email.Body = "URL: " + HttpContext.Current?.Request?.Url + Environment.NewLine + "IP: " + HttpContext.Current?.Request?.UserHostAddress + Environment.NewLine + "User: " + ApplicationEventManager.GetCurrentUserId(HttpContext.Current?.User) + Environment.NewLine + error.ToLogString(error.Message);
            Database.Save(email);
            return email;
        }
    }
}
