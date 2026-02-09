using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using System.ComponentModel;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class CrestronGenericBaseCommunicationMonitor : StatusMonitorBase
	{
		GenericBase Device;

		/// <summary>
		/// Constructor for CrestronGenericBaseCommunicationMonitor
		/// </summary>
		/// <param name="parent">parent device</param>
		/// <param name="device">device to monitor</param>
		/// <param name="warningTime">time before warning status</param>
		/// <param name="errorTime">time before error status</param>
		public CrestronGenericBaseCommunicationMonitor(IKeyed parent, GenericBase device, long warningTime, long errorTime)
			: base(parent, warningTime, errorTime)
		{
			Device = device;
		}

  /// <summary>
  /// Start method
  /// </summary>
  /// <inheritdoc />
		public override void Start()
		{
			Device.OnlineStatusChange -= Device_OnlineStatusChange;
			Device.OnlineStatusChange += Device_OnlineStatusChange;
			GetStatus();
		}

  /// <summary>
  /// Stop method
  /// </summary>
  /// <inheritdoc />
		public override void Stop()
		{
			Device.OnlineStatusChange -= Device_OnlineStatusChange;
		}	
		
		void Device_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			GetStatus();
		}

		void GetStatus()
		{
			if (Device.IsOnline)
			{
				Status = MonitorStatus.IsOk;
				StopErrorTimers();
			}
			else
				StartErrorTimers();
		}
	}
}