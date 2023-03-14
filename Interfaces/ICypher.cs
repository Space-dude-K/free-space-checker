using check_up_money.Cypher;
using System.Security;

namespace FreeSpaceChecker.Interfaces
{
    interface ICypher
    {
        RequisiteInformation Encrypt(
            SecureString user, SecureString pass);
        RequisiteInformation Decrypt(
            SecureString userEncrypted, string uSalt, SecureString passEncrypted, string pSalt);
        string ToInsecureString(SecureString input);
        SecureString ToSecureString(string input);
        SecureString DecryptString(SecureString encryptedData, string salt);
    }
}