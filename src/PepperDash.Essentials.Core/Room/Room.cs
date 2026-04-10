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

	/// <summary>
	/// Abstract base class for Room
	/// </summary>
	public abstract class Room : Device, IHasFeedback
	{
		/// <summary>
		/// Gets or sets the RoomIsOnFeedback
		/// </summary>
		public abstract BoolFeedback RoomIsOnFeedback { get; protected set; }

		/// <summary>
		/// Gets or sets the IsCoolingDownFeedback
		/// </summary>
		public abstract BoolFeedback IsCoolingDownFeedback { get; protected set; }

		/// <summary>
		/// Gets or sets the IsWarmingUpFeedback
		/// </summary>
		public abstract BoolFeedback IsWarmingUpFeedback { get; protected set; }

		// In concrete classes, these should be computed from the relevant devices
		/// <summary>
		/// Gets or sets the CooldownTime
		/// </summary>
		/// <inheritdoc />
		public virtual uint CooldownTime { get { return 10000; } }

		/// <summary>
		/// Gets or sets the WarmupTime
		/// </summary>
		/// <inheritdoc />
		public virtual uint WarmupTime { get { return 5000; } }

		/// <summary>
		/// Gets or sets the Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the HelpMessage
		/// </summary>
		public string HelpMessage { get; set; }

		/// <summary>
		/// Room Constructor
		/// </summary>
		/// <param name="key">room key</param>
		/// <param name="name">room name</param>
		public Room(string key, string name)
			: base(key, name)
		{
			Description = "";
			HelpMessage = "";
		}

  /// <summary>
  /// RoomOn method
  /// </summary>
  /// <inheritdoc />
		public virtual void RoomOn() { }

  /// <summary>
  /// RoomOff method
  /// </summary>
		public virtual void RoomOff() { }

		#region IDeviceWithOutputs Members

		/// <summary>
		/// Gets the Feedbacks
		/// </summary>
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