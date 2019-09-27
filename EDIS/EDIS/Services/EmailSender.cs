using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;

namespace EDIS.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private SmtpClient smtp;
        public EmailSender()
        {
            smtp = new SmtpClient("172.28.25.5");
        }

        public Task<string> SendEmailAsync(string from, string[] cc, string[] to, 
                            string subject, string message)
        {
            string result = "";
            MailMessage msg = new MailMessage();
            msg.Body = message;
            msg.Subject = subject;
            msg.From = new MailAddress(from);
            foreach (string t in to)
            {
                msg.To.Add(t);
            }
            foreach (string c in cc)
            {
                msg.CC.Add(c);
            }
            try
            {
                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                msg.Dispose();
                smtp.Dispose();
            }
            return Task.FromResult<string>(result);
        }
    }
}
