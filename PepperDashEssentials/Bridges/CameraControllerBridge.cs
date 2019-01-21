using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

namespace PepperDash.Essentials.Bridges
{
	public static class CameraControllerApiExtensions
	{

		public static BasicTriList _TriList;
		public static CameraControllerJoinMap JoinMap; 
		public static void LinkToApi(this PepperDash.Essentials.Devices.Common.Cameras.CameraBase cameraDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			JoinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as CameraControllerJoinMap;
			
			_TriList = trilist;
			if (JoinMap == null)
			{
				JoinMap = new CameraControllerJoinMap();
			}
			
			JoinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to Bridge Type {0}", cameraDevice.GetType().Name.ToString());

			var commMonitor = cameraDevice as ICommunicationMonitor;
			commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[JoinMap.IsOnline]);


			trilist.SetBoolSigAction(JoinMap.Left, (b) =>
				{
					if (b)
					{
						cameraDevice.PanLeft();
					}
					else
					{
						cameraDevice.Stop();
					}
				});
			trilist.SetBoolSigAction(JoinMap.Right, (b) =>
			{
				if (b)
				{
					cameraDevice.PanRight();
				}
				else
				{
					cameraDevice.Stop();
				}
			});

			trilist.SetBoolSigAction(JoinMap.Up, (b) =>
			{
				if (b)
				{
					cameraDevice.TiltUp();
				}
				else
				{
					cameraDevice.Stop();
				}
			});
			trilist.SetBoolSigAction(JoinMap.Down, (b) =>
			{
				if (b)
				{
					cameraDevice.TiltDown();
				}
				else
				{
					cameraDevice.Stop();
				}
			});

			trilist.SetBoolSigAction(JoinMap.ZoomIn, (b) =>
			{
				if (b)
				{
					cameraDevice.ZoomIn();
				}
				else
				{
					cameraDevice.Stop();
				}
			});

			trilist.SetBoolSigAction(JoinMap.ZoomOut, (b) =>
			{
				if (b)
				{
					cameraDevice.ZoomOut();
				}
				else
				{
					cameraDevice.Stop();
				}
			});


			if (cameraDevice.GetType().Name.ToString().ToLower() == "cameravisca")
			{
				var viscaCamera = cameraDevice as PepperDash.Essentials.Devices.Common.Cameras.CameraVisca;
				trilist.SetSigTrueAction(JoinMap.PowerOn, () => viscaCamera.PowerOn());
				trilist.SetSigTrueAction(JoinMap.PowerOff, () => viscaCamera.PowerOff());

				viscaCamera.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[JoinMap.PowerOn]);
				viscaCamera.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[JoinMap.PowerOff]);

				viscaCamera.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[JoinMap.IsOnline]);
				for (int i = 0; i < JoinMap.NumberOfPresets; i++)
					{	
						int tempNum = i;
						trilist.SetSigTrueAction((ushort)(JoinMap.PresetRecallOffset + tempNum), () =>
							{
								viscaCamera.RecallPreset(tempNum);
							});
						trilist.SetSigTrueAction((ushort)(JoinMap.PresetSaveOffset + tempNum), () =>
						{
							viscaCamera.SavePreset(tempNum);
						});
					}
			}

			
			
		}


	}
	public class CameraControllerJoinMap : JoinMapBase
	{
		public uint IsOnline { get; set; }
		public uint PowerOff { get; set; }
		public uint PowerOn { get; set; }
		public uint Up { get; set; }
		public uint Down { get; set; }
		public uint Left { get; set; }
		public uint Right { get; set; }
		public uint ZoomIn { get; set; }
		public uint ZoomOut { get; set; }
		public uint PresetRecallOffset { get; set; }
		public uint PresetSaveOffset { get; set; }
		public uint NumberOfPresets { get; set;  }

		public CameraControllerJoinMap()
		{
			// Digital
			IsOnline = 9;
			PowerOff = 8;
			PowerOn = 7;
			Up = 1;
			Down = 2;
			Left = 3;
			Right = 4;
			ZoomIn = 5;
			ZoomOut = 6;
			PresetRecallOffset = 10;
			PresetSaveOffset = 30;
			NumberOfPresets = 5;
			// Analog
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;

			IsOnline = IsOnline + joinOffset;
			PowerOff = PowerOff + joinOffset;
			PowerOn = PowerOn + joinOffset;
			Up = Up + joinOffset;
			Down = Down + joinOffset;
			Left = Left + joinOffset;
			Right = Right + joinOffset;
			ZoomIn = ZoomIn + joinOffset;
			ZoomOut = ZoomOut + joinOffset;
			PresetRecallOffset = PresetRecallOffset + joinOffset;
			PresetSaveOffset = PresetSaveOffset + joinOffset;
		}
	}
}