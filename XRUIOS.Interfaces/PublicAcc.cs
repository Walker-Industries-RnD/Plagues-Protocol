using MagicOnion;
using System.Runtime.Serialization;

namespace XRUIOS.Interfaces
{
    [DataContract]
    public struct PublicAccount
    {
        [DataMember] public string Name;
        [DataMember] public string LastCheck;
        [DataMember] public string OSFolder;

        public PublicAccount(string name, string lastCheck, string oSFolder)
        {
            Name = name;
            LastCheck = lastCheck;
            OSFolder = oSFolder;
        }
    }

    public interface IPublicAcc : IService<IPublicAcc>
    {
        UnaryResult<PublicAccount> GetAccInfo(string Acc);
    }



}
