using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronLogger;
using Serilog.Core;
using Serilog.Events;

namespace PepperDash.Core.Logging
{
    /// <summary>
    /// Represents a DebugCrestronLoggerSink
    /// </summary>
    public class DebugCrestronLoggerSink : ILogEventSink
    {
        /// <summary>
        /// Emit method
        /// </summary>
        public void Emit(LogEvent logEvent)
        {
            if (!Debug.IsRunningOnAppliance) return;

            string message = $"[{logEvent.Timestamp}][{logEvent.Level}][App {InitialParametersClass.ApplicationNumber}]{logEvent.RenderMessage()}";

            if (logEvent.Properties.TryGetValue("Key", out var value) && value is ScalarValue sv && sv.Value is string rawValue)
            {
                message = $"[{logEvent.Timestamp}][{logEvent.Level}][App {InitialParametersClass.ApplicationNumber}][{rawValue}]: {logEvent.RenderMessage()}";
            }

            CrestronLogger.WriteToLog(message, (uint)logEvent.Level);
        }

        /// <summary>
        /// Constructor for DebugCrestronLoggerSink
        /// </summary>
        public DebugCrestronLoggerSink()
        {
            CrestronLogger.Initialize(1, LoggerModeEnum.RM);
        }
    }
}
