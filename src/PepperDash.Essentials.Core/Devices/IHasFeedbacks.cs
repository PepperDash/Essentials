using System;
using System.Linq;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

	public interface IHasFeedback : IKeyed
	{
		/// <summary>
		/// This method shall return a list of all Output objects on a device,
		/// including all "aggregate" devices.
		/// </summary>
		FeedbackCollection<Feedback> Feedbacks { get; }

	}


	public static class IHasFeedbackExtensions
	{
		public static void DumpFeedbacksToConsole(this IHasFeedback source, bool getCurrentStates)
		{
        Type t = source.GetType();
        // get the properties and set them into a new collection of NameType wrappers
        var props = t.GetProperties().Select(p => new PropertyNameType(p, t));

			var feedbacks = source.Feedbacks;
			if (feedbacks != null)
			{
				Debug.LogMessage(LogEventLevel.Information, source, "\n\nAvailable feedbacks:");
				foreach (var f in feedbacks)
				{
					string val = "";
                string type = "";
					if (getCurrentStates)
					{
                    if (f is BoolFeedback)
                    {
                        val = f.BoolValue.ToString();
                        type = "boolean";
                    }
                    else if (f is IntFeedback)
                    {
                        val = f.IntValue.ToString();
                        type = "integer";
                    }
                    else if (f is StringFeedback)
                    {
                        val = f.StringValue;
                        type = "string";
                    }
					}
					Debug.LogMessage(LogEventLevel.Information, "{0,-12} {1, -25} {2}", type,
						(string.IsNullOrEmpty(f.Key) ? "-no key-" : f.Key), val);
				}
			}
			else
				Debug.LogMessage(LogEventLevel.Information, source, "No available outputs:");
		}
	}