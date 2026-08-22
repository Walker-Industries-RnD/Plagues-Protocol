using XRUIOS.Interfaces;

namespace XRUIOS.Linux
{
    public class Accounts
    {
        // SecureStore key the Linux worker publishes its bound address under.
        private const string WorkerName = "XRUIOS.Linux.PublicAccDataHandler";

        // Requires the worker's Manager-held PSK — runs inside the Manager, never an arbitrary app.
        public async Task<PublicAccount?> GetAccData(string accountName, byte[] workerPsk)
        {
            try
            {
                var serviceAddr = Utils.SecureStore.Get<string>(WorkerName)
                    ?? throw new Exception($"Worker '{WorkerName}' address not found in SecureStore. Is the worker running?");

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
