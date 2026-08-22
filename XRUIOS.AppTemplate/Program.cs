using XRUIOS.Interfaces;

// ═══════════════════════════════════════════════════════════════════════════════════════════════
//  XRUIOS APP TEMPLATE  —  copy this project to start a new app.
//
//  What an XRUIOS app is:
//    • It is LAUNCHED BY the XRUIOS.Manager — never run standalone. The Manager hands it its
//      credentials over inherited environment (parent→child), so nothing secret touches disk.
//    • It talks ONLY to the Manager broker. It never sees a worker, never holds the master key.
//    • It may call any capability the Manager GRANTED it; permission is enforced Manager-side by
//      XRUIOS.Permission. A refusal comes back as an exception.
//
//  To register this app with the Manager (in the Manager's Program.cs), do what it does for the
//  Calendar app:
//    1. var cred = apps.Register("myapp");                 // mint appId + per-app PSK (held in-Manager)
//    2. await permissions.GrantAsync(cred.AppId, "Cap");   // grant each capability it may call
//    3. launch this exe with env XRUIOS_APP_ID / XRUIOS_APP_PSK / XRUIOS_BROKER_ADDR set
//       (the Manager's app-launch step already does this; add your exe to its launch list)
// ═══════════════════════════════════════════════════════════════════════════════════════════════

// 1. Read the credentials the Manager handed us at launch. If they're missing, we weren't launched
//    by the Manager — refuse to run (an app can't self-provision).
string? appId = Environment.GetEnvironmentVariable("XRUIOS_APP_ID");
string? pskBase64 = Environment.GetEnvironmentVariable("XRUIOS_APP_PSK");
string? brokerAddress = Environment.GetEnvironmentVariable("XRUIOS_BROKER_ADDR");

if (appId is null || pskBase64 is null || brokerAddress is null)
{
    Console.Error.WriteLine("Launch me through the XRUIOS.Manager — no credentials in the environment.");
    return 1;
}
byte[] appPsk = Convert.FromBase64String(pskBase64);

// 2. Open ONE secure session to the Manager broker and keep it for the app's lifetime.
//    Handshake once; reuse the warm session for every call (don't reconnect per request).
await using var xruios = await EclipseSecureClient.ConnectAsync(
    brokerAddress, clientName: appId, psk: appPsk, identity: appId);

Console.WriteLine($"[{appId}] Connected to XRUIOS.");

// 3. Call capabilities. The Manager routes each call to whichever worker exposes it, after checking
//    your permissions. You just name the capability and pass its arguments.
try
{
    // ── TODO: replace with your app's real calls. ──────────────────────────────────────────────
    // InvokeAsync<TReturn>(capabilityName, argsByName). Example against the sample account worker:
    //
    //   var acc = await xruios.InvokeAsync<PublicAccount>(
    //       "GetAccInfo", new Dictionary<string, object?> { ["accountName"] = Environment.UserName });
    //   Console.WriteLine(acc);
    //
    // A capability you weren't granted throws "XRUIOS.Permission denied '<cap>' ...".

    var result = await xruios.InvokeAsync<string>(
        "SomeCapability", new Dictionary<string, object?> { ["arg1"] = "value" });
    Console.WriteLine($"Result: {result}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Call failed or was denied: {ex.Message}");
}

return 0;
