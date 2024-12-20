using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DMZ.DebugSystem
{
    //TODO: add logging to WEB
    public static class DMZLogger
    {
        public class LoggerOptions
        {
            public bool ShowErrorsAsModalView;
            public bool ShowWarningAsModalView;
            public bool ToFileByDefault;
        }

        public enum Severity
        {
            All = 6,
            Debug = 5,
            Normal = 4,
            Warn = 3,
            Error = 2,
            Fatal = 1,
            None = 0
        }

        public static LoggerOptions Options = new LoggerOptions();
        public static event Action<string> OnError;
        public static event Action<string> OnWarning;

        private static string LogSourceNameColor = "#ffffff";
        private static string LogLineColor = "#9fbfc9";

#if UNITY_EDITOR

        static DMZLogger()
        {
            if (UnityEditor.EditorGUIUtility.isProSkin)
                return;

            LogSourceNameColor = "#242424";
            LogLineColor = "#1b3f4a";
        }

#endif

        private static string _logFolderPath = Path.Combine(Application.persistentDataPath, "Logs");

        public static void SetOptions(LoggerOptions options) => Options = options ?? new LoggerOptions();

        public static void Log(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) =>
            Log(message?.ToString(), memberName, sourceFilePath, sourceLineNumber);

        public static void Log(string message = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            //var logMessage = $"%c{Path.GetFileName(sourceFilePath)}: {message}\n{memberName}:{sourceLineNumber}";
            //var css = "color: white; font-weight: bold;";
            //Application.ExternalCall("console.log", logMessage, css);
            var logMessage = $"{Path.GetFileName(sourceFilePath)}: {message}\n{memberName}:{sourceLineNumber}";
            Application.ExternalCall("console.log", logMessage);
#else
            var logMessage = PrepareLogMessage(message, sourceFilePath, memberName, sourceLineNumber);
            ToFileIfDefault(logMessage);
            Debug.Log(logMessage);
#endif
        }

        public static void LogError(string message = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = PrepareLogMessage(message, sourceFilePath, memberName, sourceLineNumber);
            ToFileIfDefault(logMessage);
            Debug.LogError(logMessage);
            OnError?.Invoke(logMessage);
        }

        public static void LogError(Exception exception,
            string customMessage = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = PrepareLogMessage(string.IsNullOrEmpty(customMessage)
                    ? exception.ToString()
                    : (customMessage + $"\n-------------\n{exception}"),
                sourceFilePath, memberName, sourceLineNumber);

            ToFileIfDefault(logMessage);

            Debug.LogError(logMessage);
            OnError?.Invoke(logMessage);
        }

        public static void LogWarning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var logMessage = $"%c{Path.GetFileName(sourceFilePath)}: {message}\n{memberName}:{sourceLineNumber}";
            var css = "color: yellow; font-weight: bold;";
            Application.ExternalCall("console.log", logMessage, css);
#else
            var logMessage = PrepareLogMessage(message, sourceFilePath, memberName, sourceLineNumber);
            ToFileIfDefault(logMessage);
            Debug.LogWarning(logMessage);
#endif
            OnWarning?.Invoke(logMessage);
        }

        public static void ToFile(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = PrepareLogMessage(message, sourceFilePath, memberName, sourceLineNumber);

            using (var stream = new StreamWriter(_logFolderPath, true))
            {
                stream.WriteLine(TimeWrapMessage(logMessage));
            }
        }

        public static void ToFile(Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = PrepareLogMessage(exception.Message, sourceFilePath, memberName, sourceLineNumber);

            using var stream = new StreamWriter(_logFolderPath, true);
            stream.WriteLine(TimeWrapMessage(logMessage));
            stream.WriteLine("RR.Trace:");
            stream.WriteLine(exception.StackTrace);
            stream.WriteLine("RR.EndOfTrace -------------------------------");
        }

        private static void ToFileIfDefault(string message)
        {
            if (!Options.ToFileByDefault)
                return;

            ToFile(message);
        }

        private static string TimeWrapMessage(string message)
            => $"{DateTime.UtcNow.ToLongTimeString()}\n{message}";

        private static string PrepareLogMessage(
            string message,
            string sourceFilePath,
            string memberName,
            int sourceLineNumber) =>
            $"<color={LogSourceNameColor}><b>{Path.GetFileName(sourceFilePath)}:</b></color> {message}\n<color={LogLineColor}>{memberName}:{sourceLineNumber}</color>";
    }
}