using Crestron.SimplSharpPro;


namespace PepperDash.Essentials.Core.Monitoring
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