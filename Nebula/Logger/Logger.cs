using System.Text;
using BepInEx;
using BepInEx.Logging;

namespace Nebula.Logger;

public static class Logger
{
    private static ManualLogSource? logSource { get; set; }

    internal static void SetLogSource(ManualLogSource Source)
    {
        if (ConsoleManager.ConsoleEnabled) System.Console.OutputEncoding = Encoding.UTF8;
        logSource = Source;
    }

    public static void Info(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Info);
    public static void Message(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Message);
    public static void Warn(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Warning);
    public static void Error(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Error);
    public static void Debug(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Debug);
    public static void Fatal(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Fatal);

    public static void SendLog(string? text, string tag = "", LogLevel logLevel = LogLevel.Info)
    {
        string time = DateTime.Now.ToString("HH:mm:ss");
        if (!string.IsNullOrWhiteSpace(tag)) text = $"[{time}] [{tag}] {text}";
        else text = $"[{time}] {text}";

        switch (logLevel)
        {
            case LogLevel.Message:
                logSource?.LogMessage(text);
                break;
            case LogLevel.Error:
                logSource?.LogError(text);
                break;
            case LogLevel.Warning:
                logSource?.LogWarning(text);
                break;
            case LogLevel.Fatal:
                logSource?.LogFatal(text);
                break;
            case LogLevel.Info:
                logSource?.LogInfo(text);
                break;
            case LogLevel.Debug:
                logSource?.LogDebug(text);
                break;
            default:
                logSource?.LogInfo(text);
                break;
        }
    }
}