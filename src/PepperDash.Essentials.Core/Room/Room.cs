using PepperDash.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Room
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

		public virtual FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
                return new FeedbackCollection<Feedback>
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