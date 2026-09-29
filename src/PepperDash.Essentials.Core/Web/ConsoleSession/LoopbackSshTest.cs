using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using PepperDash.Core;
using Interlocked = System.Threading.Interlocked;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace PepperDash.Essentials.Core.Web.ConsoleSession
{
    /// <summary>
    /// Console command that checks Essentials can hold an SSH session to its own processor console.
    /// </summary>
    /// <remarks>
    /// Usage: sshloopbacktest &lt;username&gt; &lt;password&gt; [count] [intervalMs]
    /// Opens SSH to 127.0.0.1:22, sends "ver" <c>count</c> times on the same session, and times each
    /// response by waiting for the console prompt to reappear. Nothing is printed until the session is
    /// closed, since console output printed while connected would be streamed back into the session.
    /// </remarks>
    public static class LoopbackSshTest
    {
        private const string Command = "ver";
        private const int DefaultCount = 20;
        private const int DefaultIntervalMs = 50;
        private const int PromptSettleMs = 1500;
        private const int ResponseTimeoutMs = 5000;

        private static int _running;

        /// <summary>
        /// Registers the console command
        /// </summary>
        /// <remarks>
        /// Runs during startup, so a failure here is logged instead of thrown: an exception would stop the
        /// rest of Essentials from loading.
        /// </remarks>
        public static void Register()
        {
            try
            {
                if (!CrestronConsole.AddNewConsoleCommand(Run, "sshloopbacktest",
                    "SSH to this processor's console: <user> <pass> [count] [ms]",
                    ConsoleAccessLevelEnum.AccessOperator))
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Warning, "Unable to register sshloopbacktest console command");
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception registering sshloopbacktest console command");
            }
        }

        private static void Run(string args)
        {
            var parts = (args ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                CrestronConsole.ConsoleCommandResponse("Usage: sshloopbacktest <username> <password> [count] [intervalMs]\r\n");
                return;
            }

            var count = parts.Length > 2 && int.TryParse(parts[2], out var c) && c > 0 ? c : DefaultCount;
            var intervalMs = parts.Length > 3 && int.TryParse(parts[3], out var i) && i >= 0 ? i : DefaultIntervalMs;

            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                CrestronConsole.ConsoleCommandResponse("sshloopbacktest is already running\r\n");
                return;
            }

            CrestronConsole.ConsoleCommandResponse(
                $"sshloopbacktest: connecting to 127.0.0.1:22 as {parts[0]}, {count} x '{Command}' every {intervalMs}ms. Results print when the session closes.\r\n");

            Task.Run(() =>
            {
                try
                {
                    CrestronConsole.PrintLine(Execute(parts[0], parts[1], count, intervalMs));
                }
                catch (Exception ex)
                {
                    CrestronConsole.PrintLine($"sshloopbacktest failed: {ex.Message}");
                }
                finally
                {
                    Interlocked.Exchange(ref _running, 0);
                }
            });
        }

        private static string Execute(string username, string password, int count, int intervalMs)
        {
            var report = new StringBuilder();
            var received = new StringBuilder();
            var receivedLock = new object();
            var dataArrived = new AutoResetEvent(false);
            var drops = 0;
            var connected = false;

            var ssh = new GenericSshClient("ssh-loopback-test", "127.0.0.1", 22, username, password)
            {
                AutoReconnect = false
            };

            ssh.TextReceived += (s, e) =>
            {
                lock (receivedLock) received.Append(e.Text);
                dataArrived.Set();
            };

            ssh.ConnectionChange += (s, e) =>
            {
                if (e.Client.IsConnected) connected = true;
                else if (connected) drops++;
            };

            var connectTimer = Stopwatch.StartNew();
            ssh.Connect();
            connectTimer.Stop();

            if (!ssh.IsConnected)
            {
                report.AppendLine($"sshloopbacktest: CONNECT FAILED after {connectTimer.ElapsedMilliseconds}ms (status {ssh.ClientStatus}).");
                report.Append("Check the credentials and the Essentials log for 'ssh-loopback-test' errors.");
                return report.ToString();
            }

            try
            {
                // Let the login banner and first prompt arrive, then take the prompt from the last line
                Thread.Sleep(PromptSettleMs);

                string banner;
                lock (receivedLock) banner = received.ToString();

                var prompt = banner.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .LastOrDefault(l => l.EndsWith(">"));

                if (prompt == null)
                {
                    report.AppendLine($"sshloopbacktest: connected in {connectTimer.ElapsedMilliseconds}ms, but no console prompt seen.");
                    report.Append($"Received {banner.Length} chars: {Escape(banner, 300)}");
                    return report.ToString();
                }

                var latencies = new List<long>();
                var timeouts = 0;

                for (var n = 0; n < count && ssh.IsConnected; n++)
                {
                    int promptsBefore;
                    lock (receivedLock) promptsBefore = CountOccurrences(received.ToString(), prompt);

                    var timer = Stopwatch.StartNew();
                    ssh.SendText(Command + "\r\n");

                    var answered = false;
                    while (timer.ElapsedMilliseconds < ResponseTimeoutMs)
                    {
                        lock (receivedLock)
                        {
                            if (CountOccurrences(received.ToString(), prompt) > promptsBefore)
                            {
                                answered = true;
                                break;
                            }
                        }

                        dataArrived.WaitOne(50);
                    }

                    timer.Stop();

                    if (answered) latencies.Add(timer.ElapsedMilliseconds);
                    else timeouts++;

                    if (intervalMs > 0) Thread.Sleep(intervalMs);
                }

                var stillConnected = ssh.IsConnected;
                int totalChars;
                lock (receivedLock) totalChars = received.Length;

                report.AppendLine("sshloopbacktest results");
                report.AppendLine($"  Connect:     {connectTimer.ElapsedMilliseconds}ms, prompt '{prompt}'");
                report.AppendLine($"  Commands:    {latencies.Count + timeouts} sent, {latencies.Count} answered, {timeouts} timed out (>{ResponseTimeoutMs}ms)");

                if (latencies.Count > 0)
                {
                    report.AppendLine($"  Latency:     min {latencies.Min()}ms, avg {latencies.Average():F0}ms, max {latencies.Max()}ms");
                }

                report.AppendLine($"  Received:    {totalChars} chars");
                report.AppendLine($"  Drops:       {drops}");
                report.Append($"  Session:     {(stillConnected ? "still connected at end" : "DISCONNECTED before end")}");

                return report.ToString();
            }
            finally
            {
                ssh.Disconnect();
            }
        }

        private static int CountOccurrences(string text, string value)
        {
            var total = 0;
            var index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                total++;
                index += value.Length;
            }

            return total;
        }

        private static string Escape(string text, int maxLength)
        {
            var trimmed = text.Length > maxLength ? text.Substring(text.Length - maxLength) : text;
            return trimmed.Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}
