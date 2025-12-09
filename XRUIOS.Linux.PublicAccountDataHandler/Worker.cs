using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Pariah_Cybersecurity;
using System.Diagnostics;
using System.IO;
using XRUIOS.Interfaces;
using static Pariah_Cybersecurity.EasyPQC;

namespace XRUIOS.Linux.PublicAccountDataHandler
{
    public class PublicAccService : ServiceBase<IPublicAcc>, IPublicAcc
    {
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

            // Bind server address to environment variable for untrusted side
            var addresses = _server.Features.Get<IServerAddressesFeature>();
            if (addresses != null && addresses.Addresses.Any())
            {
                var address = addresses.Addresses.First();
                Environment.SetEnvironmentVariable("XRUIOS_WORKER_PUBLICACCDATAHANDLER", address);
                _logger.LogInformation("PublicAccDataHandler bound at {address}", address);
            }
            else
            {
                _logger.LogWarning("Could not find server addresses.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Linux Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public async Task<bool> VerifyIntegrity()
        {
            try
            {
                string assemblyLoc = typeof(PublicAccount).Assembly.Location;
                _logger.LogInformation("Verifying assembly: {path}", assemblyLoc);
                byte[] hashBytes;
                using (var stream = new FileStream(assemblyLoc, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    hashBytes = await FileOperations.HashFile(stream);
                }
                using (var trustedStream = new FileStream(assemblyLoc, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool result = await FileOperations.VerifyHash(trustedStream, hashBytes);
                    if (!result) _logger.LogWarning("VerifyHash failed for {path}", assemblyLoc);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("VerifyIntegrity Exception: {msg}", ex.Message);
                return false;
            }
        }

        public async Task<bool> VerifyIntegrity2()
        {
            try
            {
                // Linux equivalent: verify the worker's own executable
                string exePath = Process.GetCurrentProcess().MainModule?.FileName
                                 ?? "/proc/self/exe"; // fallback for some Linux environments

                if (!File.Exists(exePath))
                    throw new FileNotFoundException($"Could not determine current executable path: {exePath}");

                // ---- EXE Hash + Verify ----
                byte[] exeHash;
                using (var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    exeHash = await FileOperations.HashFile(fs);

                using (var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool exeValid = await FileOperations.VerifyHash(fs, exeHash);
                    if (!exeValid) throw new Exception($"EXE integrity verification failed: {exePath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("VerifyIntegrity2 failed: {msg}", ex.Message);
                throw new Exception("VerifyIntegrity2 failed: " + ex.Message, ex);
            }
        }

        private async Task Start()
        {
            try
            {
                if (!await VerifyIntegrity())
                    throw new Exception("Integrity Check 1 Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup integrity check failed.");
            }
        }

        public async UnaryResult<PublicAccount> GetAccInfo(string accountName)
        {
            if (!await VerifyIntegrity2())
                throw new Exception("Integrity Check 2 Failed");

            var folder = $@"/home/{accountName}/XRUIOS";
            var lastCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _logger.LogInformation("[Linux] Requested info for account: {account}", accountName);

            return new PublicAccount(accountName, lastCheck, folder);
        }
    }
}