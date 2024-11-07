using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Timers
{
	public static class JobTimer
	{
		static CTimer MinuteTimer;

		static List<JobTimerItem> Items = new List<JobTimerItem>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="act"></param>
		public static void AddAction(Action act)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="act"></param>
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
	/// 
	/// </summary>
	public class JobTimerItem
	{
		public string Key { get; private set; }
		public Action JobAction { get; private set; }
		public eJobTimerCycleTypes CycleType { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime RunNextAt { get; set; }

		public JobTimerItem(string key, eJobTimerCycleTypes cycle, Action act)
		{

		}
	}

	public enum eJobTimerCycleTypes
	{
        RunEveryDay,
		RunEveryHour,
		RunEveryHalfHour,
		RunEveryMinute
	}
}