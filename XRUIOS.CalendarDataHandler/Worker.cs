using EclipseProject;

namespace XRUIOS.CalendarDataHandler
{
    // A second Plagues worker: a privileged Calendar data handler. Exposes two capabilities so the
    // example app can show one it's allowed to call (GetEvents) and one it isn't (DeleteAllEvents) —
    // the difference enforced by XRUIOS.Permission at the Manager, not here. WorkerOcean scans this
    // assembly for these [SeaOfDirac] methods.
    public static class CalendarCapabilities
    {
        // Synthetic data so the demo is self-contained.
        private static readonly Dictionary<string, string[]> Events = new()
        {
            ["2026-08-08"] = new[] { "09:00 Standup", "13:00 Lunch w/ Kennaness", "15:30 Notary review" },
            ["2026-08-09"] = new[] { "11:00 Ship Plagues v2" },
        };

        [SeaOfDirac("GetEvents", new[] { "day" }, typeof(string), typeof(string))]
        public static string GetEvents(string day)
        {
            Console.WriteLine($"[Calendar] GetEvents({day})");
            if (Events.TryGetValue(day, out var list) && list.Length > 0)
                return $"{list.Length} event(s) on {day}: {string.Join("; ", list)}";
            return $"No events on {day}.";
        }

        [SeaOfDirac("DeleteAllEvents", new[] { "confirm" }, typeof(string), typeof(string))]
        public static string DeleteAllEvents(string confirm)
        {
            Console.WriteLine($"[Calendar] DeleteAllEvents({confirm}) — privileged!");
            return "All calendar events deleted.";
        }
    }
}
