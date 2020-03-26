using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SMTP_IMAP
{
     class Program
     {
          static void Main(string[] args)
          {
               SendMessage("daniel.verhovetchi@ati.utm.md", "smkfsjajfksa", "Hello world!!!");
               GetMessages("daniel.verhovetchi@ati.utm.md", "sakfjkas");
               Console.ReadKey();
          }
          
          public static void SendMessage(string email, string password, string message)
          {
               using (MailMessage mail = new MailMessage())
               {
                    mail.From = new MailAddress(email);
                    mail.To.Add(email);
                    mail.Subject = message;
                    mail.Body = "<h1>Hello</h1>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.office365.com", 587))
                    {
                         smtp.Credentials = new NetworkCredential(email, password);
                         smtp.EnableSsl = true;
                         smtp.Send(mail);
                    }
               }
          }

          public static void GetMessages(string Email, string password)
          {
               using (Imap imap = new Imap())
               {
                    imap.Connect("outlook.office365.com");
                    imap.UseBestLogin(Email, password);
                    imap.SelectInbox();
                    List<long> uids = imap.Search(Flag.All);
                    foreach (long uid in uids)
                    {
                         IMail email = new MailBuilder()
                             .CreateFromEml(imap.GetMessageByUID(uid));
                         Console.WriteLine(email.Subject);
                    }
                    imap.Close();
               }
          }


     }

}
