using System.Reflection;
using XRUIOS.Interfaces;

// XRUIOS Calendar worker. Same secured-worker boot as the account worker.
//
// The worker trusts the Manager (only the Manager holds its PSK), so the local gate is allow-all;
// per-app permission decisions (who may call GetEvents vs DeleteAllEvents) are enforced upstream by
// the Manager broker via XRUIOS.Permission.

const string ServerName = "XRUIOS.CalendarDataHandler";

await SecureWorkerHost.Run(
    serverName: ServerName,
    capabilityAssembly: Assembly.GetExecutingAssembly(),
    gate: new AllowAllPermissionGate(),
    guard: NotaryGuard.ForCurrentWorker(ServerName));
