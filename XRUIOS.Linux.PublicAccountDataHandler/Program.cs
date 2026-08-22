using System.Reflection;
using XRUIOS.Interfaces;

// XRUIOS Linux worker — "PublicAccDataHandler". Same secured-worker boot as the Windows worker.
//
// We bind an ephemeral loopback port (not a Unix socket) so client discovery is identical across
// platforms: the client just reads the published http:// address from SecureStore. Switch to a
// Unix socket by passing unixSocketPath to SecureWorkerHost.Run if you prefer.

const string ServerName = "XRUIOS.Linux.PublicAccDataHandler";

IPermissionGate gate = new CapabilityAllowListGate("GetAccInfo");
NotaryGuard guard = NotaryGuard.ForCurrentWorker(ServerName);

await SecureWorkerHost.Run(
    serverName: ServerName,
    capabilityAssembly: Assembly.GetExecutingAssembly(),
    gate: gate,
    guard: guard);
