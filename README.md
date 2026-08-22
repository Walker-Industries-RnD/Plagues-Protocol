<p align="center"> <img src="https://github.com/Walker-Industries-RnD/Plagues-Protocol/blob/main/docs/assets/plagues.png" alt="Plagues" width="60%"/> </p>



<p align="center">  **A fully local, zero-trust system that splits every privileged operation into isolated, self-defending agents so a single breach doesn't mean death.**   </p>

<p align="center"> The Plagues Protocol is the ultra-minimalist, zero-trust local RPC protocol that powers every single interaction inside the XRUIOS (Our Cross Platform Framework/OS/Abstraction Layer). </p>

<p align="center"> <strong> Windows • Linux • Easy To Add Platforms • Fully Offline • Post Quantum Computing Resistant  • No BS</strong>
</p>

<br>
<p align="center">
  <a href="https://github.com/Walker-Industries-RnD/Plagues-Protocol"><strong>View on GitHub</strong></a> •
  <a href="https://walkerindustries.xyz">Walker Industries</a> •
  <a href="https://discord.gg/H8h8scsxtH">Discord</a> •
  <a href="https://www.patreon.com/walkerdev">Patreon</a>
</p>

<p align="center">
  <a href="https://walker-industries-rnd.github.io/Plagues-Protocol/" 
     style="font-size: 1.4em; color: #58a6ff; text-decoration: none;">
    <strong> Documentation • Examples • Design </strong>
  </a>
</p>


### How it works

The Plagues Protocol enforces a cryptographically-sealed boundary on the same machine between:

- **Untrusted side** – your shell, apps, plugins (running as the logged-in user)
- **Trusted side** – tiny, manifest-protected workers running as SYSTEM/root

All communication uses MagicOnion over OS-local channels (named pipes / Unix sockets) for zero-copy performance and zero network exposure.

Trusted workers:

- Verify their own executable and DLLs against a Kyber-signed manifest (Blake3 hashes) before listening
- Refuse to run if tampered
- Require no discovery, no TLS, no handshake — if you can open the pipe and the worker self-verified, the call is allowed

**Result:** Even a complete userland compromise cannot escalate or exfiltrate privileged data.

> “A single breach can never conquer the machine.”

<br>

<div align="center">

| ![WalkerDev](https://github.com/Walker-Industries-RnD/Plagues-Protocol/blob/main/docs/assets/walkerdev.png) | ![Kennaness](https://github.com/Walker-Industries-RnD/Plagues-Protocol/blob/main/docs/assets/kennaness.png) |
|-----------------------------|-----------------------------|
| **Code by WalkerDev**<br>“Loving coding is the same as hating yourself”<br>[Discord](https://discord.gg/H8h8scsxtH) | **Art by Kennaness**<br>“When will I get my isekai?”<br>[Bluesky](https://bsky.app/profile/kennaness.bsky.social) • [ArtStation](https://www.artstation.com/kennaness) |

</div>


---

## What's In Here

| Path | What it is |
| ---- | ---------- |
| `XRUIOS.Interfaces` | Shared contracts + the secure layer (Eclipse client, enrollment, secure store). |
| `XRUIOS.Core` | Cross-platform-by-default code; selects the Windows or Linux body at runtime. |
| `XRUIOS.Windows` / `XRUIOS.Linux` | The platform-specific account bodies. |
| `XRUIOS.Windows.PublicAccountDataHandler` / `XRUIOS.Linux.PublicAccountDataHandler` | Example secured workers (public account data), one per OS. |
| `XRUIOS.CalendarDataHandler` | The canonical worker example — a Calendar capability. |
| `XRUIOS.CalendarApp` | An example app that calls it through the Manager. |
| `XRUIOS Arch Test` | A smoke test proving a plain app **can't** bypass the Manager to reach a worker. |
| `templates/` | `dotnet new` templates: `xruios-manager`, `xruios-worker`, `xruios-app`. |
| `libs/` | Compiled dependencies (Eclipse, Notary, Secure Store, Keeper of Tomes). |
| `docs/` | The documentation site (design + how-to). |

## Scaffold your own

The whole solution ships as `dotnet new` templates — install once from the repo root, then generate:

```bash
dotnet new install ./templates/xruios-manager
dotnet new install ./templates/xruios-worker
dotnet new install ./templates/xruios-app

dotnet new xruios-manager -n XRUIOS.Manager
dotnet new xruios-worker  -n XRUIOS.WeatherDataHandler --capability GetForecast
dotnet new xruios-app     -n XRUIOS.WeatherApp         --capability GetForecast
```

> Full overview → [Documentation](https://walker-industries-rnd.github.io/Plagues-Protocol/)

---

## Using The System

Apps never touch a worker directly — every call goes through the **Manager**, which authenticates the
app, checks XRUIOS.Permission, and relays it to the owning worker. (A plain app trying to connect
straight to a worker is refused — that's what `XRUIOS Arch Test` proves.)

The `xruios-app` template gives you a one-file client for exactly this:

```csharp
// creds arrive via the Manager (env handoff) or attested self-enrollment — nothing on disk
await using var xruios = await XruiosAppClient.ConnectAsync("myapp");

// call a capability by name; a capability you weren't granted is refused before it reaches a worker
string info = await xruios.CallAsync<string>("GetAccInfo", new() { ["user"] = Environment.UserName });
Console.WriteLine(info);
```

Want to build your own workers and apps?  **See the [Documentation](https://walker-industries-rnd.github.io/Plagues-Protocol/).**

---
## Other Services

Interested in using Secure Store but not so excited for Plagues Protocol? 
We separated Secure Store into it's own .DLL!

**Check It** [Here](https://github.com/Walker-Industries-RnD/Secure-Store)  

## License & Artwork

**Code:** [NON-AI MPL 2.0](https://raw.githubusercontent.com/non-ai-licenses/non-ai-licenses/main/NON-AI-MPL-2.0)  
**Artwork:** © Kennaness — **NO AI training. NO reproduction. NO exceptions.**

<img src="https://github.com/Walker-Industries-RnD/Malicious-Affiliation-Ban/blob/main/WIBan.png?raw=true" align="center" style="margin-left: 20px; margin-bottom: 20px;"/>

> Unauthorized use of the artwork — including but not limited to copying, distribution, modification, or inclusion in any machine-learning training dataset — is strictly prohibited and will be prosecuted to the fullest extent of the law.
