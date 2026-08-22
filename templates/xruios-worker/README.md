# XRUIOS Plagues Worker template

Scaffolds a secured Plagues worker — Pariah process hardening → Notary anti-tamper →
Eclipse encrypted channel → `[SeaOfDirac]` capability dispatch — in one command.

## Two shapes (`--kind`)

| `--kind`                   | You get                                                                                                                                          | Use when                                                                                                   |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- |
| **`singular`** *(default)* | ONE worker, `net9.0`, one build, no branching — the same functions on every OS.                                                                  | The logic is platform-independent. Mirrors `XRUIOS.CalendarDataHandler`.                                   |
| **`crossplatform`**        | ONE worker that **auto-switches** a Windows body and a Linux body at runtime (`OperatingSystem.Is*` + `[SupportedOSPlatform]`). Still one build. | A capability needs OS-specific APIs but you want a single deployable that does the right thing on each OS. |

Both are a single `net9.0` project — a Cross-Platform worker is one binary that runs on Windows and Linux and picks the right body itself. (There is no separate Windows-only / Linux-only project shape; that split is unnecessary when one build can auto-switch.)

## Install / update

```text
dotnet new install ./templates/xruios-worker
```

`dotnet new uninstall ./templates/xruios-worker` to remove it; re-run `install` after editing the template.

## Create a worker

Run from the **solution root** so the relative references (`..\XRUIOS.Interfaces`, `..\libs\EclipseProject.dll`) resolve — a worker is a sibling project.

```text
# Singular (default)
dotnet new xruios-worker -n XRUIOS.WeatherDataHandler --capability GetForecast

# Cross Platform (auto-switching Windows/Linux bodies)
dotnet new xruios-worker -n XRUIOS.AccountDataHandler --kind crossplatform --capability GetAccInfo
```

Then add it to the solution:

```text
dotnet sln "The XRUIOS.sln" add XRUIOS.WeatherDataHandler
```

| Parameter       | Values                       | Default    | Effect                                                                                  |
| --------------- | ---------------------------- | ---------- | --------------------------------------------------------------------------------------- |
| `-n` / `--name` | any                          | —          | Project, folder, namespace, and `ServerName` (the worker's identity / SecureStore key). |
| `--kind`        | `singular` · `crossplatform` | `singular` | One body, or an auto-switching Windows/Linux pair.                                      |
| `--capability`  | a valid C# identifier        | `DoWork`   | Name of the first `[SeaOfDirac]` capability.                                            |

## Icon (Visual Studio)

`.template.config/ide.host.json` points VS at `.template.config/icon.png` — the Plagues skull.

Drop that PNG in next to `ide.host.json` and it shows on the template's card in the New Project dialog.

## What you get

* **`Program.cs`** — the secured boot; you rarely touch it. Just `SecureWorkerHost.Run(...)`.
* **`WorkerCapabilities.cs`** — the sample `[SeaOfDirac]` capability for the shape you chose. Add more methods here; `WorkerOcean` discovers them at startup — no registration.

Then register it with the Manager (`XRUIOS.Manager`), which checksums, launches, and brokers it.