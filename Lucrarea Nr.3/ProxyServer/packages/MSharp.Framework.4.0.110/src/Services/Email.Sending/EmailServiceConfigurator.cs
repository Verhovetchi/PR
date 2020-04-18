using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Web.Configuration;

namespace MSharp.Framework.Services
{
    public class EmailServiceConfigurator : IEmailServiceConfigurator
    {
        public void Configure(SmtpClient client)
        {
            if (!(GetCurrentConfig().GetSectionGroup("system.net/mailSettings") is MailSettingsSectionGroup mailSettings)) return;

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

        static Configuration GetCurrentConfig()
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
