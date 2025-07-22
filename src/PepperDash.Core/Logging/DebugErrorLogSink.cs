using Crestron.SimplSharp;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Core.Logging
{
    /// <summary>
    /// Represents a DebugErrorLogSink
    /// </summary>
    public class DebugErrorLogSink : ILogEventSink
    {
        private ITextFormatter _formatter;

        private Dictionary<LogEventLevel, Action<string>> _errorLogMap = new Dictionary<LogEventLevel, Action<string>>
        {
            { LogEventLevel.Verbose, (msg) => ErrorLog.Notice(msg) },
            {LogEventLevel.Debug, (msg) => ErrorLog.Notice(msg) },
            {LogEventLevel.Information, (msg) => ErrorLog.Notice(msg) },
            {LogEventLevel.Warning, (msg) => ErrorLog.Warn(msg) },
            {LogEventLevel.Error, (msg) => ErrorLog.Error(msg) },
            {LogEventLevel.Fatal, (msg) => ErrorLog.Error(msg) }
        };
        /// <summary>
        /// Emit method
        /// </summary>
        public void Emit(LogEvent logEvent)
        {
            string message;

            if (_formatter == null)
            {
                var programId = CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance
                    ? $"App {InitialParametersClass.ApplicationNumber}"
                    : $"Room {InitialParametersClass.RoomId}";

                message = $"[{logEvent.Timestamp}][{logEvent.Level}][{programId}]{logEvent.RenderMessage()}";

                if (logEvent.Properties.TryGetValue("Key", out var value) && value is ScalarValue sv && sv.Value is string rawValue)
                {
                    message = $"[{logEvent.Timestamp}][{logEvent.Level}][{programId}][{rawValue}]: {logEvent.RenderMessage()}";
                }
            } else
            {
                var buffer = new StringWriter(new StringBuilder(256));

                _formatter.Format(logEvent, buffer);

                message = buffer.ToString();
            }

            if(!_errorLogMap.TryGetValue(logEvent.Level, out var handler))
            {
                return;
            }

            handler(message);
        }

        public DebugErrorLogSink(ITextFormatter formatter = null)
        {
            _formatter = formatter;
        }
    }
}
