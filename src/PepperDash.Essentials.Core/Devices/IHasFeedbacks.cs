using System;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines the contract for IHasFeedback
	/// </summary>
	public interface IHasFeedback : IKeyed
	{
		/// <summary>
		/// This method shall return a list of all Output objects on a device,
		/// including all "aggregate" devices.
		/// </summary>
		FeedbackCollection<Feedback> Feedbacks { get; }

	}

	/// <summary>
	/// Extension methods for IHasFeedback
	/// </summary>
	public static class IHasFeedbackExtensions
	{
		/// <summary>
		/// Dumps the feedbacks to the console
		/// </summary>
		/// <param name="source"></param>
		/// <param name="getCurrentStates"></param>
		public static void DumpFeedbacksToConsole(this IHasFeedback source, bool getCurrentStates)
		{
			Type t = source.GetType();
			// get the properties and set them into a new collection of NameType wrappers
			var props = t.GetProperties().Select(p => new PropertyNameType(p, t));

			var feedbacks = source.Feedbacks;

			if (feedbacks == null)
			{
				CrestronConsole.ConsoleCommandResponse("No available outputs\r\n");
				return;
			}

			CrestronConsole.ConsoleCommandResponse("Available feedbacks:\r\n");
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
				CrestronConsole.ConsoleCommandResponse($"{type,-12} {(string.IsNullOrEmpty(f.Key) ? "-no key-" : f.Key),-25} {val}\r\n");
			}
		}
	}
}