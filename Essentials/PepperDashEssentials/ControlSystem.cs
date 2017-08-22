using System;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using PepperDash.Core;
using PepperDash.Core.PortalSync;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.DM;
using PepperDash.Essentials.Fusion;

namespace PepperDash.Essentials
{
	public class ControlSystem : CrestronControlSystem
	{
		PepperDashPortalSyncClient PortalSync;

		public ControlSystem()
			: base()
		{
			Thread.MaxNumberOfUserThreads = 400;
			Global.ControlSystem = this;
			DeviceManager.Initialize(this);        
		}

		/// <summary>
		/// Git 'er goin'
		/// </summary>
		public override void InitializeSystem()
		{
            CrestronConsole.AddNewConsoleCommand(s => GoWithLoad(), "go", "Reloads configuration file",
                ConsoleAccessLevelEnum.AccessOperator);
            //CrestronConsole.AddNewConsoleCommand(s => TearDown(), "ungo", "Reloads configuration file",
            //    ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s =>
            {
                foreach (var tl in TieLineCollection.Default)
                    CrestronConsole.ConsoleCommandResponse("  {0}\r", tl);
            },
            "listtielines", "Prints out all tie lines", ConsoleAccessLevelEnum.AccessOperator);

            //GoWithLoad();
		}

		/// <summary>
		/// Do it, yo
		/// </summary>
		public void GoWithLoad()
		{
//			var thread = new Thread(o =>
//			{
    			try
				{
                    CrestronConsole.AddNewConsoleCommand(EnablePortalSync, "portalsync", "Loads Portal Sync",
                        ConsoleAccessLevelEnum.AccessOperator);

                    //PortalSync = new PepperDashPortalSyncClient();

					Debug.Console(0, "Starting Essentials load from configuration");
					ConfigReader.LoadConfig2();
					LoadDevices();
					LoadTieLines();
					LoadRooms();

					DeviceManager.ActivateAll();
					Debug.Console(0, "Essentials load complete\r" +
                        "-------------------------------------------------------------");

                    //********************************************************************
#warning Remove these test things:

                    var cdt = new SecondsCountdownTimer("timer") { SecondsToCount = 20 };
                    cdt.WasCancelled += (o,a) => Debug.Console(0, "TIMER CANCELLED FOOLS!");
                    cdt.IsRunningFeedback.OutputChange += (o, a) => Debug.Console(0, "TIMER Running={0}", cdt.IsRunningFeedback);
                    cdt.PercentFeedback.OutputChange += (o, a) => Debug.Console(0, "TIMER {0}%", cdt.PercentFeedback);
                    cdt.TimeRemainingFeedback.OutputChange += (o,a) => Debug.Console(0, "TIMER time: {0}", cdt.TimeRemainingFeedback);
                    DeviceManager.AddDevice(cdt);

                    //********************************************************************
                }
				catch (Exception e)
				{
					Debug.Console(0, "FATAL INITIALIZE ERROR. System is in an inconsistent state:\r{0}", e);
				}
//				return null;
//			}, null);
		}

        public void EnablePortalSync(string s)
        {
            if (s.ToLower() == "enable")
            {
                CrestronConsole.ConsoleCommandResponse("Portal Sync features enabled");
                PortalSync = new PepperDashPortalSyncClient();
            }
        }

		public void TearDown()
		{
			Debug.Console(0, "Tearing down existing system");
			DeviceManager.DeactivateAll();

			TieLineCollection.Default.Clear();

			foreach (var key in DeviceManager.GetDevices())
				DeviceManager.RemoveDevice(key);

			Debug.Console(0, "Tear down COMPLETE");
		}


		/// <summary>
		/// Reads all devices from config and adds them to DeviceManager
		/// </summary>
		public void LoadDevices()
		{
			foreach (var devConf in ConfigReader.ConfigObject.Devices)
			{
				Debug.Console(0, "Creating device '{0}'", devConf.Key);
				// Skip this to prevent unnecessary warnings
				if (devConf.Key == "processor")
					continue;
				
				// Try local factory first
				var newDev = DeviceFactory.GetDevice(devConf);

				// Then associated library factories
				if (newDev == null)
					newDev = PepperDash.Essentials.Devices.Common.DeviceFactory.GetDevice(devConf);
				if (newDev == null)
					newDev = PepperDash.Essentials.DM.DeviceFactory.GetDevice(devConf);
				if (newDev == null)
					newDev = PepperDash.Essentials.Devices.Displays.DisplayDeviceFactory.GetDevice(devConf);

				if (newDev != null)
					DeviceManager.AddDevice(newDev);
				else
					Debug.Console(0, "WARNING: Cannot load unknown device type '{0}', key '{1}'.", devConf.Type, devConf.Key);
			}
		}

		/// <summary>
		/// Helper method to load tie lines.  This should run after devices have loaded
		/// </summary>
		public void LoadTieLines()
		{
			// Make this reusable by clearing the TieLineCollection

			var tlc = TieLineCollection.Default;
			tlc.Clear();
			foreach (var tieLineConfig in ConfigReader.ConfigObject.TieLines)
			{
				var newTL = tieLineConfig.GetTieLine();
				if (newTL != null)
					tlc.Add(newTL);
			}
		}

		/// <summary>
		/// Reads all rooms from config and adds them to DeviceManager
		/// </summary>
		public void LoadRooms()
		{
			foreach (var roomConfig in ConfigReader.ConfigObject.Rooms)
			{
				var room = roomConfig.GetRoomObject();
				if (room != null)
				{
                    if (room is EssentialsHuddleSpaceRoom)
                    {
                        DeviceManager.AddDevice(room);

                        Debug.Console(1, "Room is EssentialsHuddleSpaceRoom, attempting to add to DeviceManager with Fusion");
                        DeviceManager.AddDevice(new EssentialsHuddleSpaceFusionSystemController((EssentialsHuddleSpaceRoom)room, 0xf1));

                        var cotija = DeviceManager.GetDeviceForKey("cotijaServer") as CotijaSystemController;

                        if (cotija != null)
                        {
                            cotija.CotijaRooms.Add(new CotijaEssentialsHuddleSpaceRoomBridge(cotija, room as EssentialsHuddleSpaceRoom));
                        }
                    }
                    else
                    {
                        Debug.Console(1, "Room is NOT EssentialsHuddleSpaceRoom, attempting to add to DeviceManager w/o Fusion");
                        DeviceManager.AddDevice(room);
                    }

				}
				else
					Debug.Console(0, "WARNING: Cannot create room from config, key '{0}'", roomConfig.Key);
			}
		}

	}
}
