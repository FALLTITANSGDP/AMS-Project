using System.Net.Mail;
using System.Net;
using AMS.ViewModels;

namespace AMS.Helpers
{
    public static class NotificationSender
    {
        public static void SendEmailToUser(EmailModel emailModel)
        {
            try
            {
                string smtpAddress = "smtp.gmail.com";
                int portNumber = 587;
                bool enableSSL = true;
                string password = "";
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailModel.FromAddress);
                    mail.To.Add(emailModel.ToAddress);
                    mail.Subject = emailModel.Subject;
                    mail.Body = emailModel.Body;
                    mail.IsBodyHtml = true;
                    //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailModel.FromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
