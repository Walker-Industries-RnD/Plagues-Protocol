using EclipseProject;
using XRUIOS.Interfaces;

namespace XRUIOS.Windows.PublicAccountDataHandler
{
    // The worker's exposed capabilities.
    //
    // Each [SeaOfDirac] method becomes callable over the Eclipse encrypted channel once the
    // permission gate approves the caller. WorkerOcean scans THIS assembly for these methods,
    // so they must live in the worker exe (not a referenced library).
    //
    // The old self-hash handshake (VerifyIntegrity/VerifyIntegrity2) is gone — integrity is now
    // enforced by NotaryGuard in SecureWorkerHost before the worker ever serves a request.
    public static class PublicAccCapabilities
    {
        [SeaOfDirac("GetAccInfo", new[] { "accountName" }, typeof(PublicAccount), typeof(string))]
        public static PublicAccount GetAccInfo(string accountName)
        {
            Console.WriteLine($"[Windows] Requested info for account: {accountName}");
            var folder = $@"C:\Users\{accountName}\XRUIOS";
            var lastCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return new PublicAccount(accountName, lastCheck, folder);
        }
    }
}
