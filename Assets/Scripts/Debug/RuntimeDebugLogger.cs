using System;
using System.IO;

public static class RuntimeDebugLogger
{
    private const string LogPath = "/Users/hkucherenko26/Documents/GitHub/SpireOfValtar/.cursor/debug.log";

    public static void Log(string location, string message, string hypothesisId, string dataJson)
    {
        try
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string payload =
                "{\"sessionId\":\"debug-session\"," +
                "\"runId\":\"run1\"," +
                "\"hypothesisId\":\"" + Escape(hypothesisId) + "\"," +
                "\"location\":\"" + Escape(location) + "\"," +
                "\"message\":\"" + Escape(message) + "\"," +
                "\"data\":" + (string.IsNullOrEmpty(dataJson) ? "{}" : dataJson) + "," +
                "\"timestamp\":" + timestamp + "}" + Environment.NewLine;

            File.AppendAllText(LogPath, payload);
        }
        catch
        {
            // Avoid throwing from debug logging.
        }
    }

    public static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
