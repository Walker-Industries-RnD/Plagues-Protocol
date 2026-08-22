using EclipseProject;
using XRUIOS.Interfaces;

namespace XRUIOS.Linux.PublicAccountDataHandler
{
    // Linux worker capabilities. See the Windows worker for the full rationale — same model,
    // Linux-specific home-folder path. WorkerOcean scans this assembly for [SeaOfDirac] methods.
    public static class PublicAccCapabilities
    {
        [SeaOfDirac("GetAccInfo", new[] { "accountName" }, typeof(PublicAccount), typeof(string))]
        public static PublicAccount GetAccInfo(string accountName)
        {
            Console.WriteLine($"[Linux] Requested info for account: {accountName}");
            var folder = $@"/home/{accountName}/XRUIOS";
            var lastCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return new PublicAccount(accountName, lastCheck, folder);
        }
    }
}
