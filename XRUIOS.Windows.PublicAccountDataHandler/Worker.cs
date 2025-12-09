using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Pariah_Cybersecurity;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using XRUIOS.Interfaces;
using static Pariah_Cybersecurity.EasyPQC;

namespace XRUIOS.Windows.PublicAccountDataHandler
{
    public class PublicAccService : ServiceBase<IPublicAcc>, IPublicAcc
    {
        // Forward every call to the real Worker instance
        private readonly Worker _worker;

        public PublicAccService(Worker worker)
        {
            _worker = worker;
        }

        public async UnaryResult<PublicAccount> GetAccInfo(string accountName)
            => await _worker.GetAccInfo(accountName); 
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IServer _server;

        public Worker(ILogger<Worker> logger, IHttpContextAccessor http, IServer server)
        {
            _logger = logger;
            _httpContext = http;
            _server = server;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Start();

            // Wait until server has bound its addresses
            var addressesFeature = _server.Features.Get<IServerAddressesFeature>();
            if (addressesFeature != null)
            {
                // Sometimes the addresses appear only after a tiny delay
                while (!addressesFeature.Addresses.Any() && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(50, stoppingToken);
                }

                if (addressesFeature.Addresses.Any())
                {
                    var address = addressesFeature.Addresses.First(); 
                    Utils.SecureStore.Set("worker_addr", address);
                    _logger.LogInformation("PublicAccDataHandler bound at {address}", address);
                }
                else
                {
                    _logger.LogWarning("Could not find server addresses even after waiting.");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Windows Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        // Setup Handshake
        // First, we ensure the DLL is trusted and not a fake
        public async Task<bool> VerifyIntegrity()
        {
            try
            {
                string assemblyLoc = typeof(PublicAccount).Assembly.Location;
                Console.WriteLine("Verifying assembly: " + assemblyLoc);
                byte[] hashBytes;
                using (var stream = new FileStream(assemblyLoc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    hashBytes = await Pariah_Cybersecurity.EasyPQC.FileOperations.HashFile(stream); // sync version
                }
                using (var trustedFileStreamed = new FileStream(assemblyLoc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var output = await EasyPQC.FileOperations.VerifyHash(trustedFileStreamed, hashBytes);
                    if (!output)
                    {
                        Console.WriteLine("VerifyHash failed for " + assemblyLoc);
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("VerifyIntegrity Exception: " + ex);
                return false;
            }
        }

        public async Task<bool> VerifyIntegrity2()
        {
            try
            {
                int pid = Process.GetCurrentProcess().Id;
                var proc = Process.GetProcessById(pid);
                string exePath = proc.MainModule.FileName;

                // Only verify the running EXE — this worker is a standalone executable
                byte[] exeHash;
                using (var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    exeHash = await FileOperations.HashFile(fs);

                using (var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool exeValid = await FileOperations.VerifyHash(fs, exeHash);
                    if (!exeValid)
                        throw new Exception($"EXE integrity verification failed: {exePath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("VerifyIntegrity2 failed: " + ex.Message, ex);
            }
        }

        private async Task Start()
        {
            try
            {
                bool integrity = await VerifyIntegrity();
                if (!integrity)
                {
                    throw new Exception("Integrity Check 1 Failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async UnaryResult<PublicAccount> GetAccInfo(string accountName)
        {
            var integrity = await VerifyIntegrity2();
            if (!integrity)
            {
                throw new Exception("Integrity Check 2 Failed");
            }
            Console.WriteLine($"[Windows] Requested info for account: {accountName}");
            var folder = $@"C:\Users\{accountName}\XRUIOS";
            var lastCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var account = new PublicAccount(accountName, lastCheck, folder);
            return account;
        }
    }
}