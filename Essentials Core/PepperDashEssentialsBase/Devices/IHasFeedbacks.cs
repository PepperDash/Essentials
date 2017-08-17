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
			var feedbacks = source.Feedbacks.OrderBy(x => x.Type);
			if (feedbacks != null)
			{
				Debug.Console(0, source, "\n\nAvailable feedbacks:");
				foreach (var f in feedbacks)
				{
					string val = "";
					if (getCurrentStates)
					{
                        if (f is BoolFeedback)
                            val = " = " + f.BoolValue;
                        else if(f is IntFeedback)
                            val = " = " + f.IntValue;
                        else if(f is StringFeedback)
                            val = " = " + f.StringValue;

                        //switch (f.Type)
                        //{
                        //    case eCueType.Bool:
                        //        val = " = " + f.BoolValue;
                        //        break;
                        //    case eCueType.Int:
                        //        val = " = " + f.IntValue;
                        //        break;
                        //    case eCueType.String:
                        //        val = " = " + f.StringValue;
                        //        break;
                        //    //case eOutputType.Other:
                        //    //    break;
                        //}
					}
					Debug.Console(0, "{0,-8} {1}{2}", f.GetType(),
						(string.IsNullOrEmpty(f.Cue.Name) ? "-none-" : f.Cue.Name), val);
				}
			}
			else
				Debug.Console(0, source, "No available outputs:");
		}
	}

    public static class IHasFeedbackFusionExtensions
    {

    }
}