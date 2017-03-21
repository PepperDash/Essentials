using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Cards;

using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
	/// <summary>
	/// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
	/// 
	/// </summary>
	public class DmChassisController : CrestronGenericBaseDevice, IRoutingInputsOutputs, IRouting//, ICardPortsDevice
	{
		public DmMDMnxn Chassis { get; private set; }

		// Need a couple Lists of generic Backplane ports
		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        //public Dictionary<uint, DmInputCardControllerBase> InputCards { get; private set; }
        //public Dictionary<uint, DmSingleOutputCardControllerBase> OutputCards { get; private set; }

		public Dictionary<uint, string> InputNames { get; set; }
		public Dictionary<uint, string> OutputNames { get; set; }
        public Dictionary<uint, DmCardAudioOutputController> VolumeControls { get; private set; }

		public const int RouteOffTime = 500;
		Dictionary<PortNumberType, CTimer> RouteOffTimers = new Dictionary<PortNumberType, CTimer>();

		/// <summary>
		/// Factory method to create a new chassis controller from config data. Limited to 8x8 right now
		/// </summary>
		public static DmChassisController GetDmChassisController(string key, string name,
			string type, DMChassisPropertiesConfig properties)
		{
			try
			{
				type = type.ToLower();
				uint ipid = properties.Control.IpIdInt; // Convert.ToUInt16(properties.Id, 16);
				DmChassisController controller = null;
				if (type == "dmmd8x8")
				{
					controller = new DmChassisController(key, name, new DmMd8x8(ipid, Global.ControlSystem));
                    // add the cards and port names
                    foreach (var kvp in properties.InputSlots)
                        controller.AddInputCard(kvp.Value, kvp.Key);
    				foreach (var kvp in properties.OutputSlots)
                        controller.AddOutputCard(kvp.Value, kvp.Key);
                    foreach (var kvp in properties.VolumeControls)
                    {
                        // get the card
                        // check it for an audio-compatible type
                        // make a something-something that will make it work
                        // retire to mountain village
                        var outNum = kvp.Key;
                        var card = controller.Chassis.Outputs[outNum].Card;
                        Audio.Output audio = null;
                        if (card is DmcHdo)
                            audio = (card as DmcHdo).Audio;
                        else if (card is Dmc4kHdo)
                            audio = (card as Dmc4kHdo).Audio;
                        if (audio == null)
                            continue;
                        // wire up the audio to something here...
                        controller.AddVolumeControl(outNum, audio);
                    }

					controller.InputNames = properties.InputNames;
					controller.OutputNames = properties.OutputNames;
					return controller;
				}
			}
			catch (System.Exception e)
			{
				Debug.Console(0, "Error creating DM chassis:\r{0}", e);
			}
			return null;
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="chassis"></param>
		public DmChassisController(string key, string name, DmMDMnxn chassis)
			: base(key, name, chassis)
		{
			Chassis = chassis;
			InputPorts = new RoutingPortCollection<RoutingInputPort>();
			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
            VolumeControls = new Dictionary<uint, DmCardAudioOutputController>();
            IsOnline.OutputChange += new EventHandler<EventArgs>(IsOnline_OutputChange);
            Chassis.DMOutputChange += new DMOutputEventHandler(Chassis_DMOutputChange);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="number"></param>
        public void AddInputCard(string type, uint number)
        {
            Debug.Console(2, this, "Adding input card '{0}', slot {1}", type, number);
            if (type == "dmc4kHd")
            {
                new Dmc4kHd(number, this.Chassis);
                AddHdmiInCardPorts(number);
            }
            else if (type == "dmc4kHdDsp")
            {
                new Dmc4kHdDsp(number, this.Chassis);
                AddHdmiInCardPorts(number);
            }
            else if (type == "dmc4kC")
            {
                new Dmc4kC(number, this.Chassis);
                AddDmInCardPorts(number);
            }
            else if (type == "dmc4kCDsp")
            {
                new Dmc4kCDsp(number, this.Chassis);
                AddDmInCardPorts(number);
            }
            else if (type == "dmcHd")
            {
                new DmcHd(number, this.Chassis);
                AddHdmiInCardPorts(number);
            }
            else if (type == "dmcHdDsp")
            {
                new DmcHdDsp(number, this.Chassis);
                AddHdmiInCardPorts(number);
            }

            else if (type == "dmcC")
            {
                new DmcC(number, this.Chassis);
                AddDmInCardPorts(number);
            }
            else if (type == "dmcCDsp")
            {
                new DmcCDsp(number, this.Chassis);
                AddDmInCardPorts(number);
            }
            else if (type == "dmcS")
            {
                new DmcS(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmMmFiber);
                AddInCardLoopPorts(number);
            }
            else if (type == "dmcSDsp")
            {
                new DmcSDsp(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmMmFiber);
                AddInCardLoopPorts(number);
            }
            else if (type == "dmcS2")
            {
                new DmcS2(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmSmFiber);
                AddInCardLoopPorts(number);
            }
            else if (type == "dmcS2Dsp")
            {
                new DmcS2Dsp(number, Chassis);
                AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmSmFiber);
                AddInCardLoopPorts(number);
            }
            else if (type == "dmcSdi")
            {
                new DmcSdi(number, Chassis);
                AddInputPortWithDebug(number, "sdiIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Sdi);
                AddOutputPortWithDebug(number, "sdiOut", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Sdi, null);
                AddInCardLoopPorts(number);
            }
        }

        void AddDmInCardPorts(uint number)
        {
            AddInputPortWithDebug(number, "dmIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmCat);
            AddInCardLoopPorts(number);
        }

        void AddHdmiInCardPorts(uint number)
        {
            AddInputPortWithDebug(number, "hdmiIn", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi);
            AddInCardLoopPorts(number);
        }

        void AddInCardLoopPorts(uint number)
        {
            AddOutputPortWithDebug(number, "hdmiLoopOut", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null);
            AddOutputPortWithDebug(number, "audioLoopOut", eRoutingSignalType.Audio, eRoutingPortConnectionType.Hdmi, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="number"></param>
        public void AddOutputCard(string type, uint number)
        {
            Debug.Console(2, this, "Adding output card '{0}', slot {1}", type, number);
            if (type == "dmc4kHdo")
            {
                new Dmc4kHdoSingle(number, Chassis);
                AddDmcHdoPorts(number);
            }
            else if (type == "dmcHdo")
            {
                new DmcHdoSingle(number, Chassis);
                AddDmcHdoPorts(number);
            }
            else if (type == "dmc4kCoHd")
            {
                new Dmc4kCoHdSingle(number, Chassis);
                AddDmcCoPorts(number);
            }
            else if (type == "dmcCoHd")
            {
                new DmcCoHdSingle(number, Chassis);
                AddDmcCoPorts(number);
            }
            else if (type == "dmcSoHd")
            {
                new DmcSoHdSingle(number, Chassis);
                AddOutputPortWithDebug(number, "dmOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(number, "dmOut2", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmMmFiber, 2 * (number - 1) + 2);

            }
            else if (type == "dmcS2oHd")
            {
                new DmcS2oHdSingle(number, Chassis);
                AddOutputPortWithDebug(number, "dmOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1);
                AddOutputPortWithDebug(number, "dmOut2", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmSmFiber, 2 * (number - 1) + 2);
            }

            else
                Debug.Console(1, this, "  WARNING: Output card type '{0}' is not available", type);
        }

        void AddDmcHdoPorts(uint number)
        {
            AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(number, "audioOut1", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(number, "hdmiOut2", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 2);
            AddOutputPortWithDebug(number, "audioOut2", eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio, 2 * (number - 1) + 2);

        }

        void AddDmcCoPorts(uint number)
        {
            AddOutputPortWithDebug(number, "dmOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(number, "hdmiOut1", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2 * (number - 1) + 1);
            AddOutputPortWithDebug(number, "dmOut2", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.DmCat, 2 * (number - 1) + 2);
        }

        /// <summary>
        /// 
        /// </summary>
        void AddInputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType)
        {
            Debug.Console(2, this, "  Adding input port '{0}'", portName);
            InputPorts.Add(new RoutingInputPort(string.Format("card{0}--{1}", cardNum, portName),
                sigType, portType, cardNum, this));
        }

        /// <summary>
        /// 
        /// </summary>
        void AddOutputPortWithDebug(uint cardNum, string portName, eRoutingSignalType sigType, eRoutingPortConnectionType portType, object selector)
        {
            Debug.Console(2, this, "  Adding output port '{0}'", portName);
            OutputPorts.Add(new RoutingOutputPort(string.Format("card{0}--{1}", cardNum, portName),
                sigType, portType, selector, this));
        }

        /// <summary>
        /// 
        /// </summary>
        void AddVolumeControl(uint number, Audio.Output audio)
        {
            VolumeControls.Add(number, new DmCardAudioOutputController(audio));
        }

        /// <summary>
        /// 
        /// </summary>
        void Chassis_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            if (args.EventId == DMOutputEventIds.VolumeEventId &&
                VolumeControls.ContainsKey(args.Number))
                VolumeControls[args.Number].VolumeEventFromChassis();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pnt"></param>
		void StartOffTimer(PortNumberType pnt)
		{
			if (RouteOffTimers.ContainsKey(pnt))
				return;
			RouteOffTimers[pnt] = new CTimer(o =>
			{
				ExecuteSwitch(0, pnt.Number, pnt.Type);
			}, RouteOffTime);
		}

		// Send out sigs when coming online
		void IsOnline_OutputChange(object sender, EventArgs e)
		{
			if (IsOnline.BoolValue)
			{
				if (InputNames != null)
					foreach (var kvp in InputNames)
						Chassis.Inputs[kvp.Key].Name.StringValue = kvp.Value;
				if (OutputNames != null)
					foreach(var kvp in OutputNames)
						Chassis.Outputs[kvp.Key].Name.StringValue = kvp.Value;
			}
		}

		#region IRouting Members

		public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType sigType)
		{
			Debug.Console(0, this, "Making an awesome DM route from {0} to {1} {2}", inputSelector, outputSelector, sigType);

			var input = Convert.ToUInt32(inputSelector); // Cast can sometimes fail
			var output = Convert.ToUInt32(outputSelector);
			// Check to see if there's an off timer waiting on this and if so, cancel
			var key = new PortNumberType(output, sigType);
            if (input == 0)
            {
                StartOffTimer(key);
            }
            else
			{
				if(RouteOffTimers.ContainsKey(key))
				{
					Debug.Console(2, this, "{0} cancelling route off due to new source", output);
					RouteOffTimers[key].Stop();
					RouteOffTimers.Remove(key);
				}
			}

			Card.DMICard inCard = input == 0 ? null : Chassis.Inputs[input];

			// NOTE THAT THESE ARE NOTS - TO CATCH THE AudioVideo TYPE
			if (sigType != eRoutingSignalType.Audio)
			{
				Chassis.VideoEnter.BoolValue = true;
				Chassis.Outputs[output].VideoOut = inCard;
			}

			if (sigType != eRoutingSignalType.Video)
			{
				Chassis.AudioEnter.BoolValue = true;
				Chassis.Outputs[output].AudioOut = inCard;
			}
		}

		#endregion
//        void AddInUseTrackerToPort(RoutingOutputPort port, eRoutingSignalType sigType)
//        {
//            port.InUseTracker.InUseFeedback.OutputChange += (o, a) =>
//            {
//#warning Can we add something here that will de-route based on breakaway, or does it even matter?
//                if (!port.InUseTracker.InUseFeedback.BoolValue)
//                    this.ExecuteSwitch(0, port.Selector, sigType);
//            };
//        }

           
        //[Obsolete]
        //public void AddInputCard(uint slot, eDmInputCardType type)
        //{
        //    var cardKey = Key + "-inputCard" + slot;
        //    switch (type)
        //    {
        //        case eDmInputCardType.Dmc4kHd:
        //            InputCards[slot] = new Dmc4kHdController(cardKey, 
        //                new Dmc4kHd(slot, this.Chassis), slot);
        //            break;
        //        case eDmInputCardType.Dmc4kHdDsp:
        //            InputCards[slot] = new Dmc4kHdDspController(cardKey,
        //                new Dmc4kHdDsp(slot, this.Chassis), slot);
        //            break;
        //        case eDmInputCardType.Dmc4kC:
        //            InputCards[slot] = new Dmc4kCController(cardKey,
        //                new Dmc4kC(slot, this.Chassis), slot);
        //            break;
        //        case eDmInputCardType.Dmc4kCDsp:
        //            InputCards[slot] = new Dmc4kCDspController(cardKey,
        //                new Dmc4kCDsp(slot, this.Chassis), slot);
        //            break;
        //    }
        //}

        //[Obsolete]
        //public void AddOutputCard(uint cardSlot, eDmOutputCardType type)
        //{
        //    var cardKey = Key + "-outputCard" + cardSlot;
        //    switch (type)
        //    {
        //        case eDmOutputCardType.Dmc4kCoHd:
        //            OutputCards[cardSlot] = new Dmc4kCoHdSingleOutputCard(cardKey,
        //                new Dmc4kCoHdSingle(cardSlot, this.Chassis), cardSlot);
        //            break;
        //        case eDmOutputCardType.DmcCoHd:
        //            OutputCards[cardSlot] = new Dmc4kHdoSingleOutputCard(cardKey,
        //                new Dmc4kHdoSingle(cardSlot, this.Chassis), cardSlot);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        ///// <summary>
        ///// Helper to get a specific port from a given attached card
        ///// </summary>
        ///// <param name="card">Named 'input-N' ('output-N' is not valid, as output cards have no inputs)</param>
        ///// <param name="port">The port name on the card, for example 'hdmiOut'</param>
        ///// <returns>Returns port or null if doesn't exist</returns>
        //public RoutingInputPort GetChildInputPort(string card, string port)
        //{
        //    return GetChildPort(card, port, false) as RoutingInputPort;
        //}

        ///// <summary>
        ///// Helper to get a specific port from a given attached card
        ///// </summary>
        ///// <param name="card">Named 'input-N' or 'output-N'</param>
        ///// <param name="port">The port name on the card, for example 'hdmiOut'</param>
        ///// <returns>Returns port or null if doesn't exist</returns>
        //public RoutingOutputPort GetChildOutputPort(string card, string port)
        //{
        //    return GetChildPort(card, port, true) as RoutingOutputPort;
        //}

        ///// <summary>
        ///// Helper for above methods
        ///// </summary>
        //RoutingPort GetChildPort(string cardKey, string portKey, bool portIsOutput)
        //{
        //    var cardTokens = cardKey.Split('-');
        //    if (cardTokens.Length != 2)
        //    {
        //        Debug.Console(0, this, "WARNING: GetChildPort cannot get port. Card parameter must be 'input-N' or 'output-N'");
        //        return null;
        //    }
        //    try
        //    {
        //        uint slotNum = Convert.ToUInt32(cardTokens[1]);
        //        var cardType = cardTokens[0].ToLower();
        //        if (cardType == "input" && InputCards.ContainsKey(slotNum))
        //        {
        //            var card = InputCards[slotNum];
        //            if (portIsOutput)
        //                return card.GetOutputPort(portKey);
        //            else
        //                return card.GetInputPort(portKey);
        //        }
        //        if (cardType == "output" && OutputCards.ContainsKey(slotNum))
        //        {
        //            var card = OutputCards[slotNum];
        //            return card.GetOutputPort(portKey);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        Debug.Console(0, this, "WARNING: GetChildPort cannot get port. Only integer card numbers are valid.");
        //    }
        //    return null;
        //}

        //public override bool CustomActivate()
        //{
        //    var tlc = TieLineCollection.Default;
        //    // Take all cards and TieLine them together with the internal ports
        //    for (uint i = 1; i <= Chassis.NumberOfInputs; i++)
        //    {
        //        // If there's a matching input card for a given input number...
        //        // This test *shouldn't* be necessary if data is consistent
        //        if (InputCards.ContainsKey(i))
        //        {
        //            // Get the ports to link
        //            var cardAudio = InputCards[i].BackplaneAudioOut;
        //            var chassisAudio = InputPorts.FirstOrDefault(p => 
        //                (uint)p.Selector == i && p.Type == eRoutingSignalType.Audio);
        //            if (chassisAudio != null)
        //                tlc.Add(new TieLine(cardAudio, chassisAudio));
        //            else
        //                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
        //                    "Backplane audio tie line creation for input card {0} failed", i);

        //            // Repeat for video link
        //            var cardVideo = InputCards[i].BackplaneVideoOut;
        //            var chassisVideo = InputPorts.FirstOrDefault(p => 
        //                (uint)p.Selector == i && p.Type == eRoutingSignalType.Video);
        //            if (cardVideo != null)
        //                tlc.Add(new TieLine(cardVideo, chassisVideo));
        //            else
        //                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
        //                    "Backplane video tie line creation for input card {0} failed", i);
        //        }
        //    }

        //    // Loop through outputs and do it again - in pairs, because FYC
        //    for (uint i = 1; i <= Chassis.NumberOfOutputs / 2; i += 1)
        //    {
        //        if (OutputCards.ContainsKey(i))
        //        {
        //            //Debug.Console(0, this, "Adding internal TieLines on output card {0}", OutputCards[i].Key);
        //            // Left side of card
        //            uint a = i * 2 - 1;
        //            tlc.Add(new TieLine(
        //                OutputPorts.First(p => (uint)p.Selector == a && p.Type == eRoutingSignalType.Audio), 
        //                OutputCards[i].BackplaneAudioIn1));
        //            tlc.Add(new TieLine(
        //                OutputPorts.First(p => (uint)p.Selector == a && p.Type == eRoutingSignalType.Video), 
        //                OutputCards[i].BackplaneVideoIn1));

        //            // Right side of card
        //            uint b = i * 2;
        //            tlc.Add(new TieLine(
        //                OutputPorts.First(p => (uint)p.Selector == b && p.Type == eRoutingSignalType.Audio),
        //                OutputCards[i].BackplaneAudioIn2));
        //            tlc.Add(new TieLine(
        //                OutputPorts.First(p => (uint)p.Selector == b && p.Type == eRoutingSignalType.Video),
        //                OutputCards[i].BackplaneVideoIn2));
        //        }
        //    }

        //    // Base does register and sets up comm monitoring.
        //    return base.CustomActivate();
        //}

	}

	public struct PortNumberType
	{
		public uint Number { get; private set; }
		public eRoutingSignalType Type { get; private set; }

		public PortNumberType(uint number, eRoutingSignalType type) : this()
		{
			Number = number;
			Type = type;
		}
	}
}