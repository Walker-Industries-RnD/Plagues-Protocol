using XRUIOS.Interfaces;

namespace XRUIOS.Windows
{
    public class Accounts
    {
        // SecureStore key the Windows worker publishes its bound address under.
        private const string WorkerName = "XRUIOS.Windows.PublicAccDataHandler";

        // Requires the worker's Manager-held PSK — this path runs inside the Manager (or a peer the
        // Manager trusted with the key), never an arbitrary app.
        public async Task<PublicAccount?> GetAccData(string accountName, byte[] workerPsk)
        {
            try
            {
                // Discover the worker's ephemeral address (published by SecureWorkerHost).
                var serviceAddr = Utils.SecureStore.Get<string>(WorkerName)
                    ?? throw new Exception($"Worker '{WorkerName}' address not found in SecureStore. Is the worker running?");

                // Open an Eclipse-secured session and call the capability over the encrypted channel.
                await using var session = await EclipseSecureClient.ConnectAsync(serviceAddr, "xruios-manager", workerPsk);

                return await session.InvokeAsync<PublicAccount>("GetAccInfo",
                    new Dictionary<string, object?> { ["accountName"] = accountName });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while getting Account Data: " + ex.Message, ex);
            }
        }
    }
}
