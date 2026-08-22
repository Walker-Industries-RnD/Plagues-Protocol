# XRUIOS.App

An **XRUIOS app** — a broker client. It connects to the XRUIOS.Manager over the Eclipse secure
channel and calls capabilities by name. It never sees a worker and never holds a master key: the
Manager authenticates it, checks XRUIOS.Permission, and relays the call.

## ⚠ Before you build — check these two references

This project references the shared Plagues layer by **relative path**, in `XRUIOS.App.csproj`:

```xml
<ProjectReference Include="..\XRUIOS.Interfaces\XRUIOS.Interfaces.csproj" />
<Reference Include="EclipseProject"><HintPath>..\libs\EclipseProject.dll</HintPath></Reference>
```

So it only builds when it sits **beside `XRUIOS.Interfaces` and `libs/` inside the Plagues-Protocol
repo** — the same place the worker projects live. If you created it somewhere else, either move this
folder next to them, or open the `.csproj` and point those two paths at your Plagues layer. Until they
resolve, the build fails with "project/file not found."

## Run it

```
dotnet run
```

`Program.cs` connects to the Manager and calls the sample capability. It needs a **running, unlocked
Manager** (and this app registered + granted); otherwise the connect/enroll step throws — that's
expected, not a bug. See the Plagues wiki, *Registering and Launching*.

## Add your own capabilities

`XruiosAppClient.cs` is the reusable client. Call anything by name:

```csharp
string result = await xruios.CallAsync<string>("SomeCapability", new() { ["arg"] = "value" });
```

Or add a strongly-typed wrapper next to the generated one (the sample was named from `--capability`):

```csharp
public Task<string> SomeCapabilityAsync(string input) =>
    CallAsync<string>("SomeCapability", new() { ["input"] = input });
```

A capability you weren't granted is refused by the Manager before it reaches a worker — catch the
exception and handle it.

## Use it in a UI instead of the console

`XruiosAppClient.cs` is framework-agnostic — no `Console`, no UI assumptions. To use it in a WPF /
Avalonia / MAUI / OpenSilver app, copy that one file into your project (keep the `namespace` matching
the project) and drive it exactly the same way.
