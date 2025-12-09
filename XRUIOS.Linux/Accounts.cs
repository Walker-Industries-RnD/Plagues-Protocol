using Grpc.Net.Client;
using MagicOnion.Client;
using XRUIOS.Interfaces;

namespace XRUIOS.Linux
{
    public class Accounts
    {
        public async Task<PublicAccount?> GetAccData(string accountName)
        {
            try
            {
                // Get the dynamically assigned worker address
                var serviceAddr = Environment.GetEnvironmentVariable("XRUIOS_WORKER_ADDR");

                // Create gRPC channel
                using var channel = GrpcChannel.ForAddress(serviceAddr);

                // Create a proxy client
                var client = MagicOnionClient.Create<IPublicAcc>(channel);

                // Call the server method
                var account = await client.GetAccInfo(accountName);

                return account;
            }

            catch (Exception ex)
            {
                throw new Exception("An error occured while getting Account Data: " + ex.Message);
            }
        }
    }
}
