using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace pdaMediaX.Net
{
    public class pdamxSendMail
    {
        public String To { get; set; }
        public String From { get; set; }
        public String Subject { get; set; }
        public String MailServerHost { get; set; }
        public int MailServerPort { get; set; }
        public String User { get; set; }
        public String Password { get; set; }
        public String Message { get; set; }
        public Boolean EnableSSL { get; set; }

        public void SendMail()
        {
            SendMail(Message);
        }
        public void SendMail(String _sMessage)
        {
            MailMessage mailMessage = new MailMessage(From, To);
            mailMessage.Subject = Subject;
            mailMessage.Body = _sMessage;

            SmtpClient smtpClient = new SmtpClient(MailServerHost, MailServerPort);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = User,
                Password = Password
            };
            smtpClient.EnableSsl = EnableSSL;
            smtpClient.Send(mailMessage);
        }
    }
} 
