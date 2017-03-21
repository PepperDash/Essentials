using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	//***************************************************************************************************

	public abstract class Room : Device, IHasFeedback
	{
		public abstract BoolFeedback RoomIsOnFeedback { get; protected set; }
		public abstract BoolFeedback IsCoolingDownFeedback { get; protected set; }
		public abstract BoolFeedback IsWarmingUpFeedback { get; protected set; }

		// In concrete classes, these should be computed from the relevant devices
		public virtual uint CooldownTime { get { return 10000; } }
		public virtual uint WarmupTime { get { return 5000; } }

		public string Description { get; set; }
		public string HelpMessage { get; set; }

		public Room(string key, string name)
			: base(key, name)
		{
			Description = "";
			HelpMessage = "";
		}

		public virtual void RoomOn() { }

		public virtual void RoomOff() { }

		#region IDeviceWithOutputs Members

		public virtual List<Feedback> Feedbacks
		{
			get
			{
				return new List<Feedback>
                {
                    RoomIsOnFeedback,
                    IsCoolingDownFeedback,
                    IsWarmingUpFeedback
                };
			}
		}

		#endregion
	}
}