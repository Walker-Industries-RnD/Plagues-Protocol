using XRUIOS.Interfaces;

// ██ HÖLLVANIA MUNICIPAL CALENDAR ██ — an example XRUIOS app, ON-LYNE circa 1999.
//
// It is a "plain app": it holds ONLY its Manager-issued credentials and the broker address. It
// never sees a worker, never holds the master key. Everything goes through the XRUIOS.Manager,
// which authenticates it, checks XRUIOS.Permission, and relays to the Calendar worker.

static void Say(ConsoleColor c, string s) { Console.ForegroundColor = c; Console.WriteLine(s); Console.ResetColor(); }
static void Type(ConsoleColor c, string s, int ms = 6)
{
    Console.ForegroundColor = c;
    foreach (char ch in s) { Console.Write(ch); Thread.Sleep(ms); }
    Console.WriteLine(); Console.ResetColor();
}

Console.OutputEncoding = System.Text.Encoding.UTF8;

Say(ConsoleColor.Green, @"
                                                                =@@@
                                                                =@@@.
                                                               -    :-
                                                                 ...
                                                               #.   +%
                                                               .     .
                                                              .%@@@@@%
                                            .--:             -%@@@@@@@-:             :--
                                            .=  =:       :+@@@@@@@@@@@@@@@=        -.  -
                                              ==  @- -#@@@@@@###########@@@@@@#  #*  =
                                                @* +@@@%#+*%@@@@@@@@@@@@@#+*#@@@@::#-
                                                .=@@@%%=-+#@@@@@@@@@@@@@@@*=:*#@@@@-
                                              -=@@###%%.  -@@@@@@@@@@@@@@@.  +%###%@%-
                                              @@*= -***    -%@@@@@@@@@@@+.   :**+..+@@+
                                            -*#*    +**      -%@@@@@@@=-     -*+-   :***
                                          =%##       -*=:      .......      -+*       :###:
                                        -@@@*-        .++:                 -+-.        =@@@@:
                                        -@@@#*         -=---.           :-:=+.        .*%@@#.
                                            +%*+         :-+=-:.......--+=:.         +##.
                                              @@:.          .-=========-           .#@+
                                              ++@@-                              .+@@+.
                                                -+@#=                          :+@%=
                                                %*:*@@%=:                   :*%@@=-#-
                                              == :@+   %@#+-:           -=+%@-  .#*..=.
                                            -+  +-        .#@@@@@@@@@@@@@+        .=: .=
                                            -+==              *@@@@@@@              .===
                                                              -@@@@@@@
                                                               =:. .-=
                                                              -%-   *%
                                                               :-===-:
                                                              .+-:::=+
                                                               -*@@@=:
                                                                #@@@:
");

Say(ConsoleColor.Green, @"");
Say(ConsoleColor.Green, @"   ╔══════════════════════════════════════════════════════════════╗");
Say(ConsoleColor.Green, @"   ║  ░▒▓  H Ö L L V A N I A   M U N I C I P A L   C A L E N D A R  ▓▒░ ║");
Say(ConsoleColor.DarkGreen, @"   ║        ON-LYNE  B B S     ·    est. 1999    ·   Y2K: [ ? ]      ║");
Say(ConsoleColor.Green, @"   ╚══════════════════════════════════════════════════════════════╝");
Say(ConsoleColor.DarkGray, "   powered by the XRUIOS.Manager  ·  Plagues Protocol secured link\n");

Thread.Sleep(150);
Type(ConsoleColor.Gray, "   > MODEM: ATDT MANAGER ................ CARRIER ESTABLISHED");
Type(ConsoleColor.Gray, "   > Fetching operator-issued credentials ....");

// Two ways to get credentials, both leaving nothing on disk:
//   • env handoff  — if the Manager spawned us as a child, our creds are already in the environment;
//   • self-enroll  — if we're a standalone exe, we ask the Manager, which ATTESTS our binary over the
//                    control pipe before handing anything back.
string? appId = Environment.GetEnvironmentVariable("XRUIOS_APP_ID");
string? pskB64 = Environment.GetEnvironmentVariable("XRUIOS_APP_PSK");
string? brokerAddr = Environment.GetEnvironmentVariable("XRUIOS_BROKER_ADDR");

if (appId == null || pskB64 == null || brokerAddr == null)
{
    Type(ConsoleColor.Gray, "   > No env handoff — enrolling with the operator (binary attestation)....");
    try
    {
        var prov = await XruiosEnrollment.EnrollAsync("calendar");
        appId = prov.AppId; pskB64 = prov.PskBase64; brokerAddr = prov.BrokerAddress;
    }
    catch (Exception ex)
    {
        Say(ConsoleColor.Red, $"\n   ✖ ENROLLMENT REFUSED: {ex.Message}");
        return;
    }
}
byte[] appPsk = Convert.FromBase64String(pskB64);
Say(ConsoleColor.DarkGreen, $"   > IDENT: {appId}    LINE: {brokerAddr}\n");

try
{
    // Handshake with the Manager broker using our per-app password + stable identity.
    // (Not `await using` — we dispose it explicitly before the bench, since the bench reuses
    //  our single appId and the server keys sessions by id: overlapping sessions would collide.)
    Type(ConsoleColor.Gray, "   > Negotiating post-quantum handshake with operator (KYBER/AES-256)....");
    var mgr = await EclipseSecureClient.ConnectAsync(
        brokerAddr, "calendar-app", appPsk, identity: appId);
    Say(ConsoleColor.Green, "   > SECURE. Nobody on this machine can read this line.\n");
    Thread.Sleep(120);

    // ---- Allowed: read the day's schedule ----
    string day = "2026-08-08";
    Type(ConsoleColor.Cyan, $"   [QUERY] GetEvents({day})  ->  routed by operator to the Calendar handler");
    string events = await mgr.InvokeAsync<string>("GetEvents",
        new Dictionary<string, object?> { ["day"] = day });

    Say(ConsoleColor.Yellow, "\n   ┌─[ TODAY IN HÖLLVANIA ]─────────────────────────────────────┐");
    foreach (var line in events.Split(';'))
        Say(ConsoleColor.White, "   │  ▸ " + line.Trim());
    Say(ConsoleColor.Yellow, "   └────────────────────────────────────────────────────────────┘\n");

    // ---- Denied: try a privileged op we were never granted ----
    Type(ConsoleColor.Cyan, "   [QUERY] DeleteAllEvents(\"yes\")  ->  requesting privileged wipe");
    try
    {
        await mgr.InvokeAsync<string>("DeleteAllEvents",
            new Dictionary<string, object?> { ["confirm"] = "yes" });
        Say(ConsoleColor.Red, "   ✖ THE WIPE WENT THROUGH — permissions are broken!");
    }
    catch (Exception ex)
    {
        Say(ConsoleColor.Red, "\n   ╳ ACCESS DENIED BY THE OPERATOR ╳");
        Say(ConsoleColor.DarkRed, "     " + ex.Message);
        Say(ConsoleColor.DarkGray, "     (this app was granted GetEvents, never DeleteAllEvents)\n");
    }

    // Done with the demo session — dispose it so the bench's appId sessions don't collide with it.
    await mgr.DisposeAsync();

    // ---- micro-bench: how fast is the secure handshake, really? ----
    Say(ConsoleColor.Cyan, "\n   [BENCH] timing the real secure handshake (loopback, warm)...");
    var sw = new System.Diagnostics.Stopwatch();

    // (a) app <-> Manager Eclipse handshake (enroll + Kyber + transcript), no Manager signature.
    //     Sequential connect→dispose, so only one appId session exists at a time.
    for (int i = 0; i < 3; i++) { var wu = await EclipseSecureClient.ConnectAsync(brokerAddr, "calendar-app", appPsk, identity: appId); await wu.DisposeAsync(); }
    var hs = new List<double>();
    for (int i = 0; i < 30; i++)
    {
        sw.Restart();
        var b = await EclipseSecureClient.ConnectAsync(brokerAddr, "calendar-app", appPsk, identity: appId);
        sw.Stop(); hs.Add(sw.Elapsed.TotalMilliseconds);
        await b.DisposeAsync();
    }
    hs.Sort();

    // (b) GetEvents on a single dedicated session — each call makes the broker do a FULL
    //     Manager<->worker handshake (incl. the Dilithium sign + verify we just added) + invoke.
    var ev = new List<double>();
    var bench = await EclipseSecureClient.ConnectAsync(brokerAddr, "calendar-app", appPsk, identity: appId);
    for (int i = 0; i < 30; i++)
    {
        sw.Restart();
        await bench.InvokeAsync<string>("GetEvents", new Dictionary<string, object?> { ["day"] = "2026-08-08" });
        sw.Stop(); ev.Add(sw.Elapsed.TotalMilliseconds);
    }
    ev.Sort();
    await bench.DisposeAsync();

    Say(ConsoleColor.White, $"      app<->Manager handshake : min {hs.First():F2}  median {hs[hs.Count / 2]:F2}  avg {hs.Average():F2} ms");
    Say(ConsoleColor.White, $"      GetEvents (+Mgr<->worker handshake w/ signature): min {ev.First():F2}  median {ev[ev.Count / 2]:F2}  avg {ev.Average():F2} ms");

    Type(ConsoleColor.DarkGreen, "\n   > LOGOFF. Stay groovy, Höllvania. //  NO CARRIER");
}
catch (Exception ex)
{
    Say(ConsoleColor.Red, $"\n   ✖ LINE DROPPED: {ex.Message}");
}
