using System.Runtime.InteropServices;
using System.Security.Cryptography;
using XRUIOS.Interfaces;

// Post-lockdown smoke test.
//
// Workers now talk to the XRUIOS.Manager ONLY: a handshake succeeds solely for the peer holding
// the worker's Manager-provisioned PSK. A plain app (like this one) has no such key, so a direct
// connection must be refused. Real apps go through the Manager, which brokers the call.
//
// To see a successful end-to-end call, run the Manager instead:
//   dotnet run --project ../XRUIOS.Manager/XRUIOS.Manager   (in the XRUIOS.Manager repo)

string workerName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "XRUIOS.Windows.PublicAccDataHandler"
    : "XRUIOS.Linux.PublicAccDataHandler";

string? workerAddr = Utils.SecureStore.Get<string>(workerName);
if (workerAddr == null)
{
    Console.WriteLine($"No worker '{workerName}' address in SecureStore — start a worker (or the Manager) first.");
    return;
}

Console.WriteLine($"Found worker at {workerAddr}");
Console.WriteLine("Attempting a DIRECT connection as a plain app (no Manager PSK)...\n");

try
{
    var noKey = RandomNumberGenerator.GetBytes(32); // an app cannot know the Manager's PSK
    await using var session = await EclipseSecureClient.ConnectAsync(workerAddr, "arch-test-app", noKey);
    await session.InvokeAsync<PublicAccount>("GetAccInfo",
        new Dictionary<string, object?> { ["accountName"] = Environment.UserName });
    Console.WriteLine("  -> UNEXPECTED: a direct app reached the worker! Lockdown is broken.");
}
catch (Exception ex)
{
    Console.WriteLine($"  -> Correctly refused — workers are Manager-only: {ex.Message}");
    Console.WriteLine("\n  Apps must request data through the XRUIOS.Manager, which authenticates");
    Console.WriteLine("  them, checks XRUIOS.Permission, and brokers the call to the worker.");
}
