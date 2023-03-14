
namespace FreeSpaceChecker.Interfaces
{
    interface IOutlookSender
    {
        void SendEmail(string textMessage, string mailSubject, string recipient);
    }
}
