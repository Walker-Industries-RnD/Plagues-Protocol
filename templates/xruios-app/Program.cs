using XRUIOS.App;

// XRUIOS app — a plain broker client. It holds only its Manager-issued credentials, connects to the
// Manager over the Eclipse post-quantum channel, and calls capabilities by name. It never sees a
// worker and never holds a master key; the Manager authenticates it, checks XRUIOS.Permission, and
// relays the call. A capability the app wasn't granted is refused before it reaches a worker.
//
// This console host is just a runnable shell around XruiosAppClient. The client itself is
// framework-agnostic — copy XruiosAppClient.cs straight into a WPF / Avalonia / MAUI / OpenSilver app
// and drive it exactly the same way.

await using var xruios = await XruiosAppClient.ConnectAsync("SampleApp");
Console.WriteLine($"Connected as {xruios.AppId}  ->  {xruios.BrokerAddress}");

try
{
    // Swap this call for your own capability + args. The generated wrapper name follows --capability.
    string result = await xruios.SampleCapabilityAsync("hello from XRUIOS.App");
    Console.WriteLine($"SampleCapability -> {result}");
}
catch (Exception ex)
{
    // Denied (not granted) or the worker refused — both surface here.
    Console.WriteLine($"Call refused or failed: {ex.Message}");
}
