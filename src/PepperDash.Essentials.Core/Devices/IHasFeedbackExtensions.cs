using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public static class IHasFeedbackExtensions
	{
		public static void DumpFeedbacksToConsole(this IHasFeedback source, bool getCurrentStates)
		{
            CType t = source.GetType();
            // get the properties and set them into a new collection of NameType wrappers
            var props = t.GetProperties().Select(p => new PropertyNameType(p, t));

			var feedbacks = source.Feedbacks;
			if (feedbacks != null)
			{
				Debug.Console(0, source, "\n\nAvailable feedbacks:");
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
					Debug.Console(0, "{0,-12} {1, -25} {2}", type,
						(string.IsNullOrEmpty(f.Key) ? "-no key-" : f.Key), val);
				}
			}
			else
				Debug.Console(0, source, "No available outputs:");
		}
	}
}