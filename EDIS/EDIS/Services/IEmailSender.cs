using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Services
{
    public interface IEmailSender
    {
        Task <string> SendEmailAsync(string from, string[] cc, string[] to, 
                            string subject, string message);
    }
}
