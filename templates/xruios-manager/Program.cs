using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using MessagePack;
using XRUIOS.Interfaces;
using XRUIOS.Manager;
using static Pariah_Cybersecurity.EasyPQC;

// XRUIOS.Manager — the trusted core. It:
//   • checksums + launches + supervises the Plagues workers,
//   • holds the MASTER key and every per-app password (never shares them),
//   • runs the broker that relays app requests to workers after an XRUIOS.Permission check.
// Apps talk to the Manager; workers talk to the Manager; nobody talks to a worker directly.

// Launcher client mode: `XRUIOS.Manager run <app>` asks a RUNNING Manager (over a local control
// pipe) to spawn a registered app, so a user can launch apps normally — an icon, a shortcut, a
// command — without hand-orchestrating the Manager. The Manager still does the secure spawn.
if (args.Length >= 2 && args[0] == "run")
{
    Console.WriteLine(await AppLauncher.RequestSpawnAsync(args[1]));
    return 0;
}

Console.WriteLine("=== XRUIOS.Manager ===\n");

// The one key the Manager never gives to anyone: encrypts the permission store.
byte[] masterKey = RandomNumberGenerator.GetBytes(32);

// Harden the trusted core on awake (Pariah AntiTamper): block injection + watch for a debugger.
SecureWorkerHost.HardenProcess(() =>
{
    Console.Error.WriteLine("[Manager] Debugger detected — wiping master key and shutting down.");
    CryptographicOperations.ZeroMemory(masterKey);
    Environment.Exit(SecureWorkerHost.TamperExit);
});

var baselineDir = Path.Combine(Path.GetTempPath(), "xruios_manager_baselines");
var permDir = Path.Combine(Path.GetTempPath(), "xruios_manager_perms");
if (Directory.Exists(permDir)) Directory.Delete(permDir, true); // fresh grants each run (demo)

// The Manager's post-quantum IDENTITY keypair. The private half never leaves this process;
// the public half is pinned into each worker so it can verify our handshake signatures.
var (managerPub, managerPriv) = await Signatures.CreateKeys();
string managerPubB64 = Convert.ToBase64String(MessagePackSerializer.Serialize(managerPub));

var notary = new NotaryVerifier(baselineDir);
var supervisor = new WorkerSupervisor(notary, managerPubB64);
var permissions = new PermissionService(permDir, masterKey);
var apps = new AppRegistry();
await permissions.EnsureStoreAsync();

// ---- 1. Start the workers (checksum → launch → register) ----
Console.WriteLine("[1] Starting workers...");
foreach (var d in ResolveWorkers())
    await supervisor.StartAsync(d);

if (supervisor.Registry.Count == 0)
{
    Console.Error.WriteLine("No workers came up. Build Plagues-Protocol so the sibling exes exist.");
    return 1;
}

Console.WriteLine("\n[2] Worker registry:");
foreach (var w in supervisor.Registry)
    Console.WriteLine($"    • {w.Announcement.Name}  caps=[{string.Join(", ", w.Announcement.Capabilities)}]  " +
                      $"pubkey={WorkerSupervisor.PublicKeyFingerprint(w.Announcement.PublicKey)}");

// ---- 2. Start the broker (per-app PSK + permission-checked routing) ----
Console.WriteLine("\n[3] Starting broker...");
var router = new BrokerRouter(supervisor, permissions, managerPriv);
var broker = await SecureWorkerHost.RunBrokerAsync(
    "XRUIOS.Manager.Broker", apps.ResolvePsk, router.RouteAsync);
string brokerAddr = Utils.SecureStore.Get<string>("XRUIOS.Manager.Broker")!;

// ---- 3. Register the Calendar app: mint creds + grant ONLY GetEvents ----
Console.WriteLine("\n[4] Registering apps + permissions...");
var calendar = apps.Register("calendar");
await permissions.GrantAsync(calendar.AppId, "GetEvents");         // allowed
// NOTE: DeleteAllEvents is deliberately NOT granted → the broker will refuse it.
Console.WriteLine($"    • {calendar.AppId}: granted [GetEvents]  (DeleteAllEvents withheld)");
// The app's PSK stays in Manager memory only — it is NOT written to SecureStore anymore.
// It is handed to the app at launch (step 6) via inherited env, the same out-of-band channel
// workers use. That closes the disk-theft impersonation the pentest found.

// ---- 3b. Certificate vault: WalkerWorks issues → Manager encrypts+signs+verifies → trust ----
Console.WriteLine("\n[5] Certificate vault (KeeperOfTomes watch + Pariah seal)...");
var vault = new CertificateVault(Path.Combine(Path.GetTempPath(), "xruios_certs"), masterKey);
await vault.InitializeAsync();

// Simulate WalkerWorks delivering a signing certificate into the watched folder.
await File.WriteAllTextAsync(
    Path.Combine(vault.IncomingDir, "calendar-app.cert"),
    "{\"appId\":\"app:calendar\",\"issuer\":\"walkerworks\",\"grants\":[\"GetEvents\"],\"issued\":\"1999-12-31\"}");

var trusted = await vault.ScanAndSealAsync();
Console.WriteLine($"    Trusted certs: {(trusted.Count == 0 ? "(none)" : string.Join(", ", trusted))}");

// ---- 4. Register launchable apps + start the on-demand launcher control channel. Apps are spawned
//         securely ON REQUEST (checksum + env-handoff), so a user starts them normally instead of the
//         Manager having to spawn them by hand at startup. ----
using var cts = new CancellationTokenSource();
Console.WriteLine("\n[6] Registering launchable apps + launcher control channel...");
var launcher = new AppLauncher(notary, brokerAddr);
string? calendarExe = GuessSiblingExe("XRUIOS.CalendarApp");
if (calendarExe != null)
{
    launcher.Register("calendar", calendarExe, calendar);
    Console.WriteLine("    • calendar  (launch on demand)");
}
launcher.StartControlServer(cts.Token);

Console.WriteLine($"\n[READY] Broker at {brokerAddr}. Master key + all app PSKs held in-Manager.");
Console.WriteLine($"        Launch an app on demand (another terminal):  XRUIOS.Manager run {string.Join(" | ", launcher.Apps)}");
Console.WriteLine("        Ctrl+C to shut everything down.\n");

// ---- Stay up as the running core ----
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
AppDomain.CurrentDomain.ProcessExit += (_, _) => supervisor.StopAll();

try { await Task.Delay(Timeout.Infinite, cts.Token); }
catch (TaskCanceledException) { }

Console.WriteLine("\nShutting down...");
await router.DisposeAsync();   // tear down warm worker sessions while workers are still alive
supervisor.StopAll();
await broker.StopAsync();
return 0;

// ---- helpers ----

static IEnumerable<WorkerDescriptor> ResolveWorkers()
{
    string accountProject = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "XRUIOS.Windows.PublicAccountDataHandler"
        : "XRUIOS.Linux.PublicAccountDataHandler";

    foreach (var project in new[] { accountProject, "XRUIOS.CalendarDataHandler" })
    {
        string? exe = GuessSiblingExe(project);
        if (exe != null)
            yield return new WorkerDescriptor
            {
                Name = Path.GetFileNameWithoutExtension(exe).Replace("PublicAccountDataHandler", "PublicAccDataHandler"),
                ExecutablePath = exe
            };
    }
}

static string? GuessSiblingExe(string project)
{
    string fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? project + ".exe" : project;
    foreach (var cfg in new[] { "Debug", "Release" })
    {
        string guess = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "Plagues-Protocol", project, "bin", cfg, "net9.0", fileName));
        if (File.Exists(guess))
            return guess;
    }
    return null;
}
