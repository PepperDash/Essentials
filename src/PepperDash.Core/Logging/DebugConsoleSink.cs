using Crestron.SimplSharp;
using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System.IO;
using System.Text;


namespace PepperDash.Core
{
    /// <summary>
    /// Represents a DebugConsoleSink
    /// </summary>
    public class DebugConsoleSink : ILogEventSink
    {
        private readonly ITextFormatter _textFormatter;

        /// <summary>
        /// Emit method
        /// </summary>
        public void Emit(LogEvent logEvent)
        {
            if (!Debug.IsRunningOnAppliance) return;            

            /*string message = $"[{logEvent.Timestamp}][{logEvent.Level}][App {InitialParametersClass.ApplicationNumber}]{logEvent.RenderMessage()}";

            if(logEvent.Properties.TryGetValue("Key",out var value) && value is ScalarValue sv && sv.Value is string rawValue)
            {
                message = $"[{logEvent.Timestamp}][{logEvent.Level}][App {InitialParametersClass.ApplicationNumber}][{rawValue,3}]: {logEvent.RenderMessage()}";
            }*/

            var buffer = new StringWriter(new StringBuilder(256));

            _textFormatter.Format(logEvent, buffer);

            var message = buffer.ToString();

            CrestronConsole.PrintLine(message);
        }

        /// <summary>
        /// Constructor for DebugConsoleSink        
        /// </summary>
        public DebugConsoleSink(ITextFormatter formatProvider )
        {
            _textFormatter = formatProvider ?? new JsonFormatter();
        }

    }

    /// <summary>
    /// Provides extension methods for DebugConsoleSink     
    /// </summary>
    public static class DebugConsoleSinkExtensions
    {
        /// <summary>
        /// DebugConsoleSink method
        /// </summary>
        public static LoggerConfiguration DebugConsoleSink(
                             this LoggerSinkConfiguration loggerConfiguration,
                                              ITextFormatter formatProvider = null)
        {
            return loggerConfiguration.Sink(new DebugConsoleSink(formatProvider));
        }
    }

}
