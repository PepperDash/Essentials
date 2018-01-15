using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Cotija
{
	public class CotijaDdvc01RoomBridge : Device
	{
		public class BoolJoin
		{
			/// <summary>
			/// 1
			/// </summary>
			public const uint GetStatus = 1;
			/// <summary>
			/// 2
			/// </summary>
			public const uint RoomIsOn = 2;
			/// <summary>
			/// 3
			/// </summary>
			public const uint DefaultSourcePress = 3;
			/// <summary>
			/// 4
			/// </summary>
			public const uint MasterVolumeIsMuted = 4;
			/// <summary>
			/// 4
			/// </summary>
			public const uint MasterVolumeMuteToggle = 4;
			/// <summary>
			/// 21
			/// </summary>
			public const uint ShutdownStart = 21;
			/// <summary>
			/// 22
			/// </summary>
			public const uint ShutdownEnd = 22;
			/// <summary>
			/// 23
			/// </summary>
			public const uint ShutdownCancel = 23;
		}

		public class UshortJoin
		{
			/// <summary>
			/// 
			/// </summary>
			public const uint MasterVolumeLevel = 4;
		}

		public class StringJoin
		{
			/// <summary>
			/// 
			/// </summary>
			public const uint SetSource = 3;
		}


		public ThreeSeriesTcpIpEthernetIntersystemCommunications EISC { get; private set; }

		CotijaSystemController Parent;

		public CotijaDdvc01RoomBridge(string key, string name, uint ipId)
			: base(key, name)
		{
			Key = key;
			try
			{
				EISC = new ThreeSeriesTcpIpEthernetIntersystemCommunications(ipId, "127.0.0.2", Global.ControlSystem);
				var reg = EISC.Register();
				if (reg != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
					Debug.Console(0, this, "Cannot connect EISC at IPID {0}: \r{1}", ipId, reg);
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Finish wiring up everything after all devices are created
		/// </summary>
		/// <returns></returns>
		public override bool CustomActivate()
		{
			
			Parent = DeviceManager.AllDevices.FirstOrDefault(d => d is CotijaSystemController) as CotijaSystemController;
			if (Parent == null)
			{
				Debug.Console(0, this, "ERROR: Cannot build CotijaDdvc01RoomBridge. System controller not present");
				return false;
			}

			SetupFunctions();
			SetupFeedbacks();
			return base.CustomActivate();
		}


		/// <summary>
		/// Setup the actions to take place on various incoming API calls
		/// </summary>
		void SetupFunctions()
		{
			Parent.AddAction(@"/room/room1/status", new Action(() => 
				EISC.PulseBool(BoolJoin.GetStatus)));
			Parent.AddAction(@"/room/room1/source", new Action<SourceSelectMessageContent>(c => 
				EISC.SetString(StringJoin.SetSource, c.SourceListItem)));
			Parent.AddAction(@"/room/room1/defaultsource", new Action(() => 
				EISC.PulseBool(BoolJoin.DefaultSourcePress)));

			Parent.AddAction(@"/room/room1/masterVolumeLevel", new Action<ushort>(u => 
				EISC.SetUshort(UshortJoin.MasterVolumeLevel, u)));
			Parent.AddAction(@"/room/room1/masterVolumeMuteToggle", new Action(() => 
				EISC.PulseBool(BoolJoin.MasterVolumeIsMuted)));

			Parent.AddAction(@"/room/room1/shutdownStart", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownStart)));
			Parent.AddAction(@"/room/room1/shutdownEnd", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownEnd)));
			Parent.AddAction(@"/room/room1/shutdownCancel", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownCancel)));
		}

		/// <summary>
		/// Links feedbacks to whatever is gonna happen!
		/// </summary>
		void SetupFeedbacks()
		{
			EISC.SetStringSigAction(StringJoin.SetSource, s =>
				PostStatusMessage(new
					{
						selectedSourceKey = s
					}));

			EISC.SetUShortSigAction(UshortJoin.MasterVolumeLevel, u =>
				PostStatusMessage(new
					{
						masterVolumeLevel = u
					}));

			EISC.SetBoolSigAction(BoolJoin.MasterVolumeIsMuted, b =>
				PostStatusMessage(new
					{
						masterVolumeMuteState = b
					}));

			EISC.SetSigTrueAction(BoolJoin.GetStatus, () =>
				PostStatusMessage(new
					{
						isOn = EISC.BooleanOutput[BoolJoin.RoomIsOn].BoolValue,
						selectedSourceKey = EISC.StringOutput[StringJoin.SetSource].StringValue,
						masterVolumeLevel = EISC.UShortOutput[UshortJoin.MasterVolumeLevel].UShortValue,
						masterVolumeMuteState = EISC.BooleanOutput[BoolJoin.MasterVolumeIsMuted].BoolValue
					}));
		}

		/// <summary>
		/// Helper for posting status message
		/// </summary>
		/// <param name="contentObject">The contents of the content object</param>
		void PostStatusMessage(object contentObject)
		{
			Parent.PostToServer(JObject.FromObject(new
				{
					type = "/room/status/",
					content = contentObject
				}));
		}
	}
}