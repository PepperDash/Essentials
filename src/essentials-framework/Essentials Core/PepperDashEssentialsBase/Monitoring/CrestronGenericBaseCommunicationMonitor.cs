﻿using System;
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

		public CrestronGenericBaseCommunicationMonitor(IKeyed parent, GenericBase device, long warningTime, long errorTime)
			: base(parent, warningTime, errorTime)
		{
			Device = device;
		}

		public override void Start()
		{
			Device.OnlineStatusChange -= Device_OnlineStatusChange;
			Device.OnlineStatusChange += Device_OnlineStatusChange;
			GetStatus();
		}

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