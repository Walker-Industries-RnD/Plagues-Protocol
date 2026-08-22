# XRUIOS Manager template

Scaffolds **the trusted core** — there is normally **one Manager per solution**. It:

* checksums (Notary), launches, and supervises the Plagues workers,
* holds the **master key** and every **per-app PSK**, and never shares them,
* runs the **broker** that relays app requests to workers *after* an XRUIOS.Permission check.

Apps talk to the Manager; workers talk to the Manager; nobody talks to a worker directly.

## Install

```text id="8v4d7r"
dotnet new install ./templates/xruios-manager
```

## Create it (do this second, after an empty solution)

```text id="x0m0tj"
dotnet new xruios-manager -n XRUIOS.Manager
dotnet sln "The XRUIOS.sln" add XRUIOS.Manager
```

> [!IMPORTANT] **Expected layout.** The generated `.csproj` references the shared Plagues layer and the permission store by relative path:
>
> ```text id="6f9xg5"
> ..\..\Plagues-Protocol\XRUIOS.Interfaces\XRUIOS.Interfaces.csproj
> ..\..\XRUIOS.Permission\PermissionHandler\PermissionHandler.csproj
> ..\..\Plagues-Protocol\libs\*.dll   (Eclipse, Notary, KeeperOfTomes)
> ```
>
> So generate the Manager into a solution folder that sits **beside `Plagues-Protocol` and `XRUIOS.Permission`** (the canonical `GitHub\<solution>\XRUIOS.Manager\` layout). If your layout differs, fix the `<ProjectReference>` / `<HintPath>` paths in the `.csproj` once.

## What's inside

The full working core, ready to run: `Program.cs` plus `WorkerSupervisor`, `BrokerRouter`, `PermissionService`, `AppRegistry`, `CertificateVault`, `NotaryVerifier`, `AppLauncher`, `WorkerSessionPool`, `WorkerDescriptor`.

`Program.cs` ships with a **worked example** (registers a `calendar` app, grants it only `GetEvents`, seals a demo certificate). Adapt these two spots for your own fleet:

1. **`ResolveWorkers()`** (bottom of `Program.cs`) — list the worker projects the Manager should checksum + launch. Point it at your `dotnet new xruios-worker` projects.
2. **Step `[4]`** — register your apps and grant capabilities (`apps.Register(...)` + `permissions.GrantAsync(...)`) instead of the calendar demo.

## The intended workflow

1. Create an **empty solution**.
2. `dotnet new xruios-manager` — the core (this template).
3. `dotnet new xruios-worker` a few times — one per capability (`singular` or `crossplatform`).
4. Point the Manager's `ResolveWorkers()` at those workers, grant your apps their capabilities, run.
