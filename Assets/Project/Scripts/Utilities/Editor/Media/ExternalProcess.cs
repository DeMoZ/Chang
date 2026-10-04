using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;

namespace Chang.Utilities.Media
{
    /// <summary>
    /// Runs a command line tool (claude, python, ffmpeg) without blocking the Editor.
    /// </summary>
    public static class ExternalProcess
    {
        // Unity started from the Hub doesn't have the shell PATH, Homebrew tools and libraries are added explicitly
        private static readonly string[] ToolFolders = { "/opt/homebrew/bin", "/usr/local/bin" };
        private const string HomebrewLibraries = "/opt/homebrew/lib";
        private const int PollMilliseconds = 300;

        public readonly struct Result
        {
            public readonly int ExitCode;
            public readonly string Output;
            public readonly bool IsCanceled;
            public readonly bool IsTimeout;

            public Result(int exitCode, string output, bool isCanceled, bool isTimeout)
            {
                ExitCode = exitCode;
                Output = output;
                IsCanceled = isCanceled;
                IsTimeout = isTimeout;
            }

            public bool IsSuccess => ExitCode == 0 && !IsCanceled && !IsTimeout;
        }

        /// <param name="isCanceled">Polled on the main thread while the process runs, true kills the process.</param>
        public static async UniTask<Result> RunAsync(string fileName, string arguments, string workingDirectory,
            TimeSpan timeout, Func<bool> isCanceled, string standardInput = null)
        {
            StringBuilder output = new();
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            SetEnvironment(process.StartInfo.Environment);

            process.OutputDataReceived += (_, e) => Append(output, e.Data);
            process.ErrorDataReceived += (_, e) => Append(output, e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!string.IsNullOrEmpty(standardInput))
            {
                await process.StandardInput.WriteAsync(standardInput);
            }

            process.StandardInput.Close();

            DateTime deadline = DateTime.Now + timeout;
            bool canceled = false;
            bool isTimeout = false;
            while (!process.HasExited)
            {
                canceled = isCanceled();
                isTimeout = !canceled && DateTime.Now > deadline;
                if (canceled || isTimeout)
                {
                    Append(output, isTimeout ? $"Timeout {timeout}" : "Canceled");
                    process.Kill();
                    break;
                }

                await UniTask.Delay(PollMilliseconds);
            }

            process.WaitForExit();

            lock (output)
            {
                return new Result(canceled || isTimeout ? -1 : process.ExitCode, output.ToString(), canceled, isTimeout);
            }
        }

        public static string FindTool(string configuredPath, params string[] candidates)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return File.Exists(configuredPath) ? configuredPath : null;
            }

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        public static string[] InToolFolders(string toolName)
        {
            return Array.ConvertAll(ToolFolders, folder => Path.Combine(folder, toolName));
        }

        private static void SetEnvironment(IDictionary<string, string> environment)
        {
            environment.TryGetValue("PATH", out string path);
            environment["PATH"] = string.Join(":", ToolFolders) + ":" + path;
            environment["DYLD_FALLBACK_LIBRARY_PATH"] = HomebrewLibraries;
        }

        private static void Append(StringBuilder output, string line)
        {
            if (line == null)
            {
                return;
            }

            lock (output)
            {
                output.AppendLine(line);
            }
        }
    }
}
