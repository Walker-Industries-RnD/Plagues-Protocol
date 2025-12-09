using XRUIOS.Core;
using XRUIOS.Interfaces;

var clientAddr = Utils.SecureStore.Get<string>("worker_addr");
if (clientAddr == null)
{
    throw new Exception("Worker address not found in secure storage.");
}
using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(clientAddr);
var client = MagicOnion.Client.MagicOnionClient.Create<XRUIOS.Interfaces.IPublicAcc>(channel);

var result = await client.GetAccInfo(Environment.UserName);

Console.Write(result);

Console.WriteLine($"Name: {result.Name}");
Console.WriteLine($"Folder: {result.OSFolder}");
Console.WriteLine($"Checked: {result.LastCheck}");
