using check_up_money.Cypher;
using System;
namespace FreeSpaceChecker.Interfaces
{
    interface ISpaceChecker
    {
        Tuple<ulong, ulong> CheckSpace(string ip, string disk, ILogger logger, RequisiteInformation req, ICypher cypher, bool isShare = false);
    }
}