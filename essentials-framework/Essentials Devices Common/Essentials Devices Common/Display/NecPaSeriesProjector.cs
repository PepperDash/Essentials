using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Devices.Displays
{
	public class NecPaSeriesProjector : ComTcpDisplayBase, IBridgeAdvanced
	{
		public readonly IntFeedback Lamp1RemainingPercent;
		int _Lamp1RemainingPercent;
		public readonly IntFeedback Lamp2RemainingPercent;
		int _Lamp2RemainingPercent;
		protected override Func<bool> PowerIsOnFeedbackFunc
		{
			get { return () => _PowerIsOn; }
		}
		bool _PowerIsOn;

		protected override Func<bool> IsCoolingDownFeedbackFunc
		{
			get { return () => false; }
		}

		protected override Func<bool> IsWarmingUpFeedbackFunc
		{
			get { return () => false; }
		}

		public override void PowerToggle()
		{
			throw new NotImplementedException();
		}

		public override void ExecuteSwitch(object selector)
		{
			throw new NotImplementedException();
		}

		Dictionary<string, string> InputMap;

		/// <summary>
		/// Constructor
		/// </summary>
		public NecPaSeriesProjector(string key, string name)
			: base(key, name)
		{
			Lamp1RemainingPercent = new IntFeedback("Lamp1RemainingPercent", () => _Lamp1RemainingPercent);
			Lamp2RemainingPercent = new IntFeedback("Lamp2RemainingPercent", () => _Lamp2RemainingPercent);

			InputMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "computer1", "\x02\x03\x00\x00\x02\x01\x01\x09" },
				{ "computer2", "\x02\x03\x00\x00\x02\x01\x02\x0a" },
				{ "computer3", "\x02\x03\x00\x00\x02\x01\x03\x0b" },
				{ "hdmi", "\x02\x03\x00\x00\x02\x01\x1a\x22" },
				{ "dp", "\x02\x03\x00\x00\x02\x01\x1b\x23" },
				{ "video", "\x02\x03\x00\x00\x02\x01\x06\x0e" },
				{ "viewer", "\x02\x03\x00\x00\x02\x01\x1f\x27" },
				{ "network", "\x02\x03\x00\x00\x02\x01\x20\x28" },
			};
		}

		void IsConnected_OutputChange(object sender, EventArgs e)
		{

		}

		public void SetEnable(bool state)
		{
			var tcp = CommunicationMethod as GenericTcpIpClient;
			if (tcp != null)
			{
				tcp.Connect();
			}
		}

		public override void PowerOn()
		{
			SendText("\x02\x00\x00\x00\x00\x02");
		}

		public override void PowerOff()
		{
			SendText("\x02\x01\x00\x00\x00\x03");
		}

		public void PictureMuteOn()
		{
			SendText("\x02\x10\x00\x00\x00\x12");
		}

		public void PictureMuteOff()
		{
			SendText("\x02\x11\x00\x00\x00\x13");
		}

		public void GetRunningStatus()
		{
			SendText("\x00\x85\x00\x00\x01\x01\x87");
		}

		public void GetLampRemaining(int lampNum)
		{
			if (!_PowerIsOn) return;

			var bytes = new byte[]{0x03,0x96,0x00,0x00,0x02,0x00,0x04};
			if (lampNum == 2)
				bytes[5] = 0x01;
			SendBytes(AppendChecksum(bytes));
		}

		public void SelectInput(string inputKey)
		{
			if (InputMap.ContainsKey(inputKey))
				SendText(InputMap[inputKey]);
		}

		void SendText(string text)
		{
			if (CommunicationMethod != null)
				CommunicationMethod.SendText(text);
		}

		void SendBytes(byte[] bytes)
		{
			if (CommunicationMethod != null)
				CommunicationMethod.SendBytes(bytes);
		}

		byte[] AppendChecksum(byte[] bytes)
		{
			byte sum = unchecked((byte)bytes.Sum(x => (int)x));
			var retVal = new byte[bytes.Length + 1];
			bytes.CopyTo(retVal, 0);
			retVal[retVal.Length - 1] = sum;
			return retVal;
		}

		protected override void CommunicationMethod_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs args)
		{
			var bytes = args.Bytes;
			ParseBytes(args.Bytes);
		}

		void ParseBytes(byte[] bytes)
		{
			if (bytes[0] == 0x22)
			{
				// Power on
				if (bytes[1] == 0x00)
				{
					_PowerIsOn = true;
					PowerIsOnFeedback.FireUpdate();
				}
				// Power off
				else if (bytes[1] == 0x01)
				{
					_PowerIsOn = false;
					PowerIsOnFeedback.FireUpdate();
				}
			}
			// Running Status
			else if (bytes[0] == 0x20 && bytes[1] == 0x85 && bytes[4] == 0x10)
			{
				var operationStates = new Dictionary<int, string>
				{
					{ 0x00, "Standby" },
					{ 0x04, "Power On" },
					{ 0x05, "Cooling" },
					{ 0x06, "Standby (error)" },
					{ 0x0f, "Standby (power saving" },
					{ 0x10, "Network Standby" },
					{ 0xff, "Not supported" }
				};

				var newPowerIsOn = bytes[7] == 0x01;
				if (newPowerIsOn != _PowerIsOn)
				{
					_PowerIsOn = newPowerIsOn;
					PowerIsOnFeedback.FireUpdate();
				}

				Debug.Console(2, this, "PowerIsOn={0}\rCooling={1}\rPowering on/off={2}\rStatus={3}",
					_PowerIsOn, 
					bytes[8] == 0x01,
					bytes[9] == 0x01,
					operationStates[bytes[10]]);
				
			}
			// Lamp remaining
			else if (bytes[0] == 0x23 && bytes[1] == 0x96 && bytes[4] == 0x06 && bytes[6] == 0x04)
			{
				var newValue = bytes[7];
				if (bytes[5] == 0x00)
				{
					if (newValue != _Lamp1RemainingPercent)
					{
						_Lamp1RemainingPercent = newValue;
						Lamp1RemainingPercent.FireUpdate();
					}
				}
				else
				{
					if (newValue != _Lamp2RemainingPercent)
					{
						_Lamp2RemainingPercent = newValue;
						Lamp2RemainingPercent.FireUpdate();
					}
				}
				Debug.Console(0, this, "Lamp {0}, {1}% remaining", (bytes[5] + 1), bytes[7]);
			}

		}

	    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
	    }
	}
}