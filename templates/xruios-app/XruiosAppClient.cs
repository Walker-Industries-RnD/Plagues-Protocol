using XRUIOS.Interfaces;

// Framework-agnostic XRUIOS app client — drop this ONE file into any C# project: a CMD tool, an
// OpenSilver page, an Avalonia / WPF / MAUI app. It is a "plain app": it holds ONLY its Manager-issued
// credentials + the broker address. It never sees a worker and never holds a master key.
//
// Everything goes through the XRUIOS.Manager, which authenticates the app, checks XRUIOS.Permission,
// and relays the call to the right Plagues worker.
//
// The host project must reference the shared Plagues layer (XRUIOS.Interfaces) so EclipseSecureClient
// + XruiosEnrollment resolve.
//
// Usage from any UI/code (no Console required):
//     await using var xruios = await XruiosAppClient.ConnectAsync("weather");
//     string forecast = await xruios.CallAsync<string>("GetForecast",
//                             new() { ["city"] = "Hollvania" });
//     // bind `forecast` to your view, etc.

namespace XRUIOS.App
{
    public sealed class XruiosAppClient : IAsyncDisposable
    {
        /// <summary>The app's registered name with the Manager (used to enroll / label the session).</summary>
        public const string DefaultAppName = "SampleApp";

        private readonly string _appId;
        private readonly byte[] _appPsk;
        private readonly string _brokerAddress;
        private EclipseSecureClient? _session;

        private XruiosAppClient(string appId, byte[] appPsk, string brokerAddress)
        {
            _appId = appId;
            _appPsk = appPsk;
            _brokerAddress = brokerAddress;
        }

        public string AppId => _appId;
        public string BrokerAddress => _brokerAddress;

        /// <summary>
        /// Acquire credentials and open ONE secure session to the Manager broker.
        ///
        /// Credentials come from one of two channels, both leaving nothing on disk:
        ///   • env handoff — if the Manager spawned this app, XRUIOS_APP_ID / XRUIOS_APP_PSK /
        ///     XRUIOS_BROKER_ADDR are already in the environment;
        ///   • self-enroll — otherwise we ask the Manager, which ATTESTS our binary before handing
        ///     anything back (XruiosEnrollment.EnrollAsync).
        /// </summary>
        public static async Task<XruiosAppClient> ConnectAsync(string appName = DefaultAppName)
        {
            string? appId = Environment.GetEnvironmentVariable("XRUIOS_APP_ID");
            string? pskB64 = Environment.GetEnvironmentVariable("XRUIOS_APP_PSK");
            string? brokerAddr = Environment.GetEnvironmentVariable("XRUIOS_BROKER_ADDR");

            if (appId is null || pskB64 is null || brokerAddr is null)
            {
                var prov = await XruiosEnrollment.EnrollAsync(appName);
                appId = prov.AppId;
                pskB64 = prov.PskBase64;
                brokerAddr = prov.BrokerAddress;
            }

            var client = new XruiosAppClient(appId, Convert.FromBase64String(pskB64), brokerAddr);
            client._session = await EclipseSecureClient.ConnectAsync(
                brokerAddr, appName + "-app", client._appPsk, identity: appId);
            return client;
        }

        /// <summary>
        /// Call a capability by name. The Manager permission-checks this app, then relays to the worker.
        /// Throws if the app was not granted the capability (or the worker refuses).
        /// </summary>
        public Task<T> CallAsync<T>(string capability, Dictionary<string, object?>? args = null)
        {
            if (_session is null)
                throw new InvalidOperationException("Not connected — call ConnectAsync first.");
            return _session.InvokeAsync<T>(capability, args ?? new Dictionary<string, object?>());
        }

        /// <summary>Example strongly-typed convenience wrapper — rename/duplicate per capability.</summary>
        public Task<string> SampleCapabilityAsync(string input) =>
            CallAsync<string>("SampleCapability", new Dictionary<string, object?> { ["input"] = input });

        public async ValueTask DisposeAsync()
        {
            if (_session is not null)
            {
                await _session.DisposeAsync();
                _session = null;
            }
        }
    }
}
