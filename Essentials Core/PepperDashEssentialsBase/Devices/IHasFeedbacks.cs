using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	public interface IHasFeedback : IKeyed
	{
		/// <summary>
		/// This method shall return a list of all Output objects on a device,
		/// including all "aggregate" devices.
		/// </summary>
		List<Feedback> Feedbacks { get; }

	}


	public static class IHasFeedbackExtensions
	{
		public static void DumpFeedbacksToConsole(this IHasFeedback source, bool getCurrentStates)
		{
			var outputs = source.Feedbacks.OrderBy(x => x.Type);
			if (outputs != null)
			{
				Debug.Console(0, source, "\n\nAvailable outputs:");
				foreach (var o in outputs)
				{
					string val = "";
					if (getCurrentStates)
					{
						switch (o.Type)
						{
							case eCueType.Bool:
								val = " = " + o.BoolValue;
								break;
							case eCueType.Int:
								val = " = " + o.IntValue;
								break;
							case eCueType.String:
								val = " = " + o.StringValue;
								break;
							//case eOutputType.Other:
							//    break;
						}
					}
					Debug.Console(0, "{0,-8} {1,5} {2}{3}", o.Type, o.Cue.Number,
						(string.IsNullOrEmpty(o.Cue.Name) ? "-none-" : o.Cue.Name), val);
				}
			}
			else
				Debug.Console(0, source, "No available outputs:");
		}
	}
}