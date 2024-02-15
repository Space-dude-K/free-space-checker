using FreeSpaceChecker.Interfaces;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;

namespace FreeSpaceChecker
{
    class MailSender : IMailSender
    {
        private readonly ILogger logger;
        private readonly ICypher cypher;

        public MailSender(ILogger logger, ICypher cypher)
        {
            this.logger = logger;
            this.cypher = cypher;
        }

        public void SendEmail(string textMessage, string mailSubject, string mailAddress, string smtpServer, string mailFrom, 
            string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt)
        {
            // create email message
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(mailFrom));
            email.To.Add(MailboxAddress.Parse(mailAddress));
            email.Subject = mailSubject;
            //email.Body = new TextPart() { Text = textMessage };
            email.Body = new TextPart(TextFormat.Html) { Text = textMessage };

            // send email
            var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                Console.WriteLine($"Connecting to -> {smtpServer}");

                smtp.Connect(smtpServer, 587, SecureSocketOptions.StartTls);

                string mailLoginS = cypher.ToInsecureString(cypher.DecryptString(cypher.ToSecureString(mailLogin), mailLoginSalt));
                string mailLoginP = cypher.ToInsecureString(cypher.DecryptString(cypher.ToSecureString(mailPassword), mailPasswordSalt));

                smtp.Authenticate(cypher.ToInsecureString(cypher.DecryptString(cypher.ToSecureString(mailLogin), mailLoginSalt)),
                    cypher.ToInsecureString(cypher.DecryptString(cypher.ToSecureString(mailPassword), mailPasswordSalt)));

                Console.WriteLine($"Sending email to -> {mailAddress}");

                smtp.Send(email);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Email error!");
                logger.Log(ex.Message);

            }
            finally
            {
                Console.WriteLine($"Disconnect from -> {smtpServer}");
                smtp.Disconnect(true);
            }   
        }
    }
}