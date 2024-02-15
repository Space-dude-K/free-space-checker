namespace FreeSpaceChecker
{
    interface IMailSender
    {
        void SendEmail(string textMessage, 
            string mailSubject, string mailAddress, 
            string smtpServer, string mailFrom,
            string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt);
    }
}