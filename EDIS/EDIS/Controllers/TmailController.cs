using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EDIS.Controllers
{
    public class Tmail
    {
        public MailAddress from;
        public MailAddress to;
        public MailAddress cc;
        public string sto;
        public MailMessage message;
        public string server;

        public Tmail()
        {
            message = new MailMessage();
            server = "172.28.25.5";
            sto = "";
        }

        public string SendMail()
        {
            string msg = "";
            SmtpClient sc = new SmtpClient(server);
            message.From = from;
            if (sto != "")
                message.To.Add(sto);
            if (to != null)
                message.To.Add(to);
            if (cc != null)
                message.Bcc.Add(cc);
            try
            {
                sc.Send(message);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally
            {
                sc.Dispose();
            }
            return msg;
        }
    }
}