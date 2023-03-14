namespace FreeSpaceChecker
{
    interface IMailSender
    {
        void SendEmail(string textMessage, string mailSubject, string mailAddress, string smtpServer, string mailFrom);
    }
}