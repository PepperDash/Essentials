using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Represents a JobTimer
	/// </summary>
	public static class JobTimer
	{
		static CTimer MinuteTimer;

		static List<JobTimerItem> Items = new List<JobTimerItem>();

		/// <summary>
		/// AddAction method
	  	/// </summary>
		/// <param name="act">action to add</param>
		public static void AddAction(Action act)
		{

		}

		/// <summary>
		/// AddJobTimerItem method
		/// </summary>
		/// <param name="item">JobTimerItem to add</param>
		public static void AddJobTimerItem(JobTimerItem item)
		{
			var existing = Items.FirstOrDefault(i => i.Key == item.Key);
			if (existing != null)
			{
				Items.Remove(existing);
			}
			Items.Add(item);
		}

		static void CheckAndRunTimer()
		{
			if (Items.Count > 0 && MinuteTimer == null)
			{
				MinuteTimer = new CTimer(o => MinuteTimerCallback(), null, 60000, 60000);
			}
		}

		static void MinuteTimerCallback()
		{
			

		}
	}

 /// <summary>
 /// Represents a JobTimerItem
 /// </summary>
	public class JobTimerItem
	{
		/// <summary>
		/// Key property
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// JobAction property
		/// </summary>
		public Action JobAction { get; private set; }

		/// <summary>
		/// CycleType property
		/// </summary>
		public eJobTimerCycleTypes CycleType { get; private set; }

		/// <summary>
		/// RunNextAt property
		/// </summary>
		public DateTime RunNextAt { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">item key</param>
		/// <param name="cycle">cycle type</param>
		/// <param name="act">action to run</param>
		public JobTimerItem(string key, eJobTimerCycleTypes cycle, Action act)
		{

		}
	}

	/// <summary>
	/// JobTimerCycleTypes enum
	/// </summary>
	public enum eJobTimerCycleTypes
	{
		/// <summary>
		/// RunEveryDay property
		/// </summary>
        RunEveryDay,

		/// <summary>
		/// RunEveryHour property
		/// </summary>
		RunEveryHour,

		/// <summary>
		/// RunEveryHalfHour property
		/// </summary>
		RunEveryHalfHour,

		/// <summary>
		/// RunEveryMinute property
		/// </summary>
		RunEveryMinute
	}
}