using System.Reflection;
using XRUIOS.Interfaces;

// XRUIOS Windows worker — "PublicAccDataHandler".
//
// Boots as an Eclipse-secured Plagues worker:
//   1. NotaryGuard verifies this folder against its baseline (anti-tamper) before listening.
//   2. SecureWorkerHost stands up the encrypted channel and publishes its address.
//   3. The permission gate (Manager-supplied in production) decides who may call what.
//
// The gate here allows only "GetAccInfo"; everything else gets refused. Swap in the
// Manager's XRUIOS.Permission-backed gate when running under the Manager.

const string ServerName = "XRUIOS.Windows.PublicAccDataHandler";

IPermissionGate gate = new CapabilityAllowListGate("GetAccInfo");
NotaryGuard guard = NotaryGuard.ForCurrentWorker(ServerName);

await SecureWorkerHost.Run(
    serverName: ServerName,
    capabilityAssembly: Assembly.GetExecutingAssembly(),
    gate: gate,
    guard: guard);
