using System.Runtime.InteropServices;
using XRUIOS.Interfaces;
#if WINDOWS
using XRUIOS.Windows;
#elif LINUX
using XRUIOS.Linux;
#endif
namespace XRUIOS.Core
{
    public static class AccountsProvider
    {
        public static async Task<PublicAccount?> GetPublicAcc(string Username)
        {
            PublicAccount? publicAcc = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var ts = new Windows.Accounts();
                publicAcc = await ts.GetAccData(Username);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var ts = new Linux.Accounts();
                publicAcc = await ts.GetAccData(Username);
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS for Accounts");
            }

            return publicAcc ?? throw new Exception("Not found");
        }
    }
}
