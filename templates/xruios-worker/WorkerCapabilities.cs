using EclipseProject;
//#if (isCross)
using System.Runtime.Versioning;
//#endif

namespace XRUIOS.WorkerName
{
    // The worker's exposed capabilities.
    //
    // Every [SeaOfDirac] method becomes callable over Eclipse's encrypted channel once the Manager's
    // permission gate approves the caller. WorkerOcean scans THIS assembly for these methods, so they
    // must live in the worker exe itself (not a referenced library).
    //
    // Attribute shape: [SeaOfDirac(name, parameterNames, returnType, params parameterTypes)].
    // Methods may be static (shown here) or instance, and may be sync or async (Task / Task<T>).
    public static class WorkerCapabilities
    {
//#if (isSingular)
        // SINGULAR worker: one body, the SAME on every OS — because the functions are the same. You
        // ship this single build everywhere and there is no Windows/Linux split to maintain. If just
        // one line ever needs to differ, branch inline with OperatingSystem.IsWindows() and you STILL
        // keep one worker.
        [SeaOfDirac("SampleCapability", new[] { "input" }, typeof(string), typeof(string))]
        public static string SampleCapability(string input)
        {
            Console.WriteLine($"[XRUIOS.WorkerName] SampleCapability({input})");
            return $"Handled '{input}' — same code path on Windows, Linux, and macOS.";
        }
//#endif
//#if (isCross)
        // CROSS-PLATFORM worker: ONE capability that auto-switches to the Windows or Linux body at
        // runtime. One build runs on both; the OperatingSystem.Is* guards let the platform analyzer
        // prove each [SupportedOSPlatform] body is only reached on its own OS (no CA1416 warnings).
        [SeaOfDirac("SampleCapability", new[] { "input" }, typeof(string), typeof(string))]
        public static string SampleCapability(string input)
        {
            if (OperatingSystem.IsWindows()) return SampleCapabilityWindows(input);
            if (OperatingSystem.IsLinux())   return SampleCapabilityLinux(input);
            throw new PlatformNotSupportedException("No SampleCapability body for this OS.");
        }

        // Windows body — free to call Windows-only APIs (registry, WMI, DirectoryServices, …).
        [SupportedOSPlatform("windows")]
        private static string SampleCapabilityWindows(string input)
        {
            Console.WriteLine($"[XRUIOS.WorkerName] (Windows) SampleCapability({input})");
            return $@"[Windows] {input} -> C:\Users\{input}\XRUIOS";
        }

        // Linux body — free to call Linux-only APIs (PAM, getpwent, journald, …).
        [SupportedOSPlatform("linux")]
        private static string SampleCapabilityLinux(string input)
        {
            Console.WriteLine($"[XRUIOS.WorkerName] (Linux) SampleCapability({input})");
            return $"[Linux] {input} -> /home/{input}/.xruios";
        }
//#endif
    }
}
