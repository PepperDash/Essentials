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
		/// This method returns a list of all Output objects on a device,
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
		/// Gets the feedback type name for sorting purposes
		/// </summary>
		/// <param name="feedback">The feedback to get the type name for</param>
		/// <returns>A string representing the feedback type</returns>
		private static string GetFeedbackTypeName(Feedback feedback)
		{
			if (feedback is BoolFeedback)
				return "boolean";
			else if (feedback is IntFeedback)
				return "integer";
			else if (feedback is StringFeedback)
				return "string";
			else
				return feedback.GetType().Name;
		}

		/// <summary>
		/// Dumps the feedbacks to the console
		/// </summary>
		/// <param name="source"></param>
		/// <param name="getCurrentStates"></param>
		public static void DumpFeedbacksToConsole(this IHasFeedback source, bool getCurrentStates)
		{
			var feedbacks = source.Feedbacks;

			if (feedbacks == null || feedbacks.Count == 0)
			{
				CrestronConsole.ConsoleCommandResponse("No available feedbacks\r\n");
				return;
			}

			CrestronConsole.ConsoleCommandResponse("Available feedbacks:\r\n");

			// Sort feedbacks by type first, then by key
			var sortedFeedbacks = feedbacks.OrderBy(f => GetFeedbackTypeName(f)).ThenBy(f => string.IsNullOrEmpty(f.Key) ? "" : f.Key);

			foreach (var feedback in sortedFeedbacks)
			{
				string value = "";
				string type = "";
				if (getCurrentStates)
				{
					if (feedback is BoolFeedback)
					{
						value = feedback.BoolValue.ToString();
						type = "boolean";
					}
					else if (feedback is IntFeedback)
					{
						value = feedback.IntValue.ToString();
						type = "integer";
					}
					else if (feedback is StringFeedback)
					{
						value = feedback.StringValue;
						type = "string";
					}
				}
				CrestronConsole.ConsoleCommandResponse($"  {type,-12} {(string.IsNullOrEmpty(feedback.Key) ? "-no key-" : feedback.Key),-25} {value}\r\n");
			}
		}
	}
}