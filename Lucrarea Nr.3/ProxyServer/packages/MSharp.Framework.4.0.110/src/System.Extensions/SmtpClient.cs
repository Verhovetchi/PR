namespace System
{
    using Collections.Concurrent;
    using Collections.Generic;
    using Linq;
    using Linq.Expressions;
    using Reflection;
    using Runtime.Remoting.Messaging;
    using System.Net.Configuration;
    using System.Web.Configuration;
    using System.Collections;
    using Threading;
    using Threading.Tasks;
    using System.Net.Mail;
    using System.Net;
    using System.IO;
    using System.Configuration;

    partial class MSharpExtensionsWeb
    {
        /// <summary>
        /// Configures this smtp client with the specified config file path.
        /// </summary>
        public static void Configure(this SmtpClient client)
        {
            var mailSettings = GetCurrentConfig().GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            if (mailSettings == null) return;

            client.Port = mailSettings.Smtp.Network.Port;

            if (mailSettings.Smtp.Network.TargetName.HasValue())
                client.TargetName = mailSettings.Smtp.Network.TargetName;

            if (client.DeliveryMethod == SmtpDeliveryMethod.Network)
                client.Host = mailSettings.Smtp.Network.Host;

            if (mailSettings.Smtp.Network.DefaultCredentials && mailSettings.Smtp.Network.UserName.HasValue() &&
                 mailSettings.Smtp.Network.Password.HasValue())
            {
                client.Credentials = new NetworkCredential(mailSettings.Smtp.Network.UserName, mailSettings.Smtp.Network.Password);
            }
        }

        static System.Configuration.Configuration GetCurrentConfig()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config")))
            {
                return WebConfigurationManager.OpenWebConfiguration(@"~\Web.Config");
            }
            else
            {
                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
        }
    }
}
