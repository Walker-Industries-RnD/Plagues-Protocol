# XRUIOS App Template

Starting point for an app that runs on XRUIOS. Copy this folder, rename it, and fill in `Program.cs`.

## The rules an app lives by

- **The Manager launches you.** You never run standalone. Your credentials (`XRUIOS_APP_ID`,
  `XRUIOS_APP_PSK`, `XRUIOS_BROKER_ADDR`) arrive via inherited environment — never disk, never
  SecureStore. If they're absent, refuse to run.
- **You only talk to the Manager broker.** You never see a worker, never hold the master key.
- **You can call only what you were granted.** Permission is enforced Manager-side by
  XRUIOS.Permission; a refusal comes back as an exception.
- **Handshake once.** Open one `EclipseSecureClient` session and reuse it — don't reconnect per call.

## Registering with the Manager

In the Manager's `Program.cs`, do the same three things it does for the Calendar app:

```csharp
var cred = apps.Register("myapp");                  // mint appId + per-app PSK (held in-Manager)
await permissions.GrantAsync(cred.AppId, "MyCap");  // grant each capability it may call
// then launch this exe with XRUIOS_APP_ID / XRUIOS_APP_PSK / XRUIOS_BROKER_ADDR in its env
```

The Manager's app-launch step already sets those env vars and Notary-checksums the binary before
starting it — add your exe to its launch list.

## Calling capabilities

```csharp
var result = await xruios.InvokeAsync<TReturn>(
    "CapabilityName", new Dictionary<string, object?> { ["argName"] = value });
```

The Manager routes the call to whichever worker exposes `CapabilityName`. To expose a *new*
capability, add a `[SeaOfDirac]` method to a worker (see `XRUIOS.CalendarDataHandler`), not here —
apps consume capabilities, workers provide them.
