using FreeSpaceChecker.Interfaces;
using System.Net.Mail;

namespace FreeSpaceChecker
{
    class MailSender : IMailSender
    {
        private readonly ILogger logger;
        public MailSender(ILogger logger)
        {
            this.logger = logger;
        }
        public void SendEmail(string textMessage, string mailSubject, string mailAddress, string smtpServer, string mailFrom)
        {
            MailMessage message = new MailMessage();

            message.To.Add(mailAddress);
            message.Subject = mailSubject;
            message.From = new MailAddress(mailFrom);
            message.IsBodyHtml = true;
            message.Body = textMessage;

            SmtpClient smtp = new SmtpClient(smtpServer);

            smtp.Send(message);
        }
    }
}