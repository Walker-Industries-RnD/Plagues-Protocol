<p align="center"> <img src="https://github.com/Walker-Industries-RnD/Plagues-Protocol/blob/main/docs/assets/plagues.png" alt="Plagues" width="80%"/> </p>



 **A fully local, zero-trust system that splits every privileged operation into isolated, self-defending agents so a single breach doesn't mean death.**  

<p align="center"> The Plagues Protocol is the ultra-minimalist, zero-trust local RPC protocol that powers every single interaction inside the XRUIOS (Our Cross Platform Framework/OS/Abstraction Layer). </p>

<p align="center">
  <strong>Windows • Linux • Easy To Add Platforms • Fully Offline • Post Quantum Computing Resistant  • No BS</strong>
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

<br>
<p align="center">
  <a href="https://github.com/Walker-Industries-RnD/Plagues-Protocol"><strong>View on GitHub</strong></a> •
  <a href="https://walkerindustries.xyz">Walker Industries</a> •
  <a href="https://discord.gg/H8h8scsxtH">Discord</a> •
  <a href="https://www.patreon.com/walkerdev">Patreon</a>
</p>

<p align="center">
  <a href="https://walker-industries-rnd.github.io/Plagues-Protocol/1.welcome/welcome.html" 
     style="font-size: 1.4em; color: #58a6ff; text-decoration: none;">
    <strong>Documentation • Examples • Design </strong>
  </a>
</p>

---

## What's In Here

| Focus        | Description                                                                          |
| ------------ | ------------------------------------------------------------------------------------ |
| Worker.cs    | A Windows Specific Service Worker Example                                            |
| Worker.cs    | A Linux Specific Service Worker Example                                              |
| PublicAcc.cs | The MagicOnion Server Interface And Definitions Shared Between Linux/Windows Workers |
| Accounts.cs  | The Windows specific function for getting the data from the Service Worker           |
| Accounts.cs  | The Linux specific function for getting the data from the Service Worker             |
| Core.cs      | Where code cross platform by default goes + the XRUIOS.Windows or Linux              |
| Program.cs   | A CMD Test Of The XRUIOS.Core                                                        |
|              |                                                                                      |


> Full overview → [[1. Design Philosophy]]

---

## Using The System

```csharp
//The Base DLL (Ensure you also have all other DLLs for your OS at minimum)
//Also Shared References Between OSes

using XRUIOS.Core;
using XRUIOS.Interfaces;
```
```csharp
//We assume the worker "XRUIOS.Windows.PublicAccountDataHandler" was launched already at start, which should have resulted in the address "worker_addr" being populated with the API endpoint we want to 

//If the worker address is not exist, you can wait for it to be set but for now we handle the exception
var clientAddr = Utils.SecureStore.Get<string>("worker_addr");
if (clientAddr == null)
{
    throw new Exception("Worker address not found in secure storage.");
}
```

```csharp
//Take the resulting address and setup an Onion Client

using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(clientAddr);
var client = MagicOnion.Client.MagicOnionClient.Create<XRUIOS.Interfaces.IPublicAcc>(channel);
```
```csharp
//Get the resulting data and ensure it shows something
var result = await client.GetAccInfo(Environment.UserName);

Console.Write(result);

Console.WriteLine($"Name: {result.Name}");
Console.WriteLine($"Folder: {result.OSFolder}");
Console.WriteLine($"Checked: {result.LastCheck}");
```
```
```

Want to understand how to make your own services?  **Check Out [[1. Setup The Plagues Protocol]]**

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
