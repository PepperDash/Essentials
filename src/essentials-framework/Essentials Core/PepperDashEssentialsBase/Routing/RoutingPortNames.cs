using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Routing
{
	/// <summary>
	/// These should correspond directly with the portNames var in the config tool.
	/// </summary>
	public class RoutingPortNames
	{
        /// <summary>
        /// antennaIn
        /// </summary>
		public const string AntennaIn = "antennaIn";
        /// <summary>
        /// anyAudioIn
        /// </summary>
		public const string AnyAudioIn = "anyAudioIn";
        /// <summary>
        /// anyAudioOut
        /// </summary>
		public const string AnyAudioOut = "anyAudioOut";
        /// <summary>
        /// anyOut
        /// </summary>
        public const string AnyOut = "anyOut";
        /// <summary>
        /// anyVideoIn
        /// </summary>
        public const string AnyVideoIn = "anyVideoIn";
        /// <summary>
        /// anyVideoOut
        /// </summary>
		public const string AnyVideoOut = "anyVideoOut";
        /// <summary>
        /// balancedAudioOut
        /// </summary>
		public const string BalancedAudioOut = "balancedAudioOut";
        /// <summary>
        /// codecOsd
        /// </summary>
        public const string CodecOsd = "codecOsd";
        /// <summary>
        /// componentIn
        /// </summary>
		public const string ComponentIn = "componentIn";
        /// <summary>
        /// componentOut
        /// </summary>
		public const string ComponentOut = "componentOut";
        /// <summary>
        /// compositeIn
        /// </summary>
		public const string CompositeIn = "compositeIn";
        /// <summary>
        /// compositeOut
        /// </summary>
		public const string CompositeOut = "compositeOut";
        /// <summary>
        /// displayPortIn
        /// </summary>
		public const string DisplayPortIn = "displayPortIn";
        /// <summary>
        /// displayPortIn1
        /// </summary>
		public const string DisplayPortIn1 = "displayPortIn1";
        /// <summary>
        /// displayPortIn2
        /// </summary>
		public const string DisplayPortIn2 = "displayPortIn2";
        /// <summary>
        /// displayPortIn3
        /// </summary>
		public const string DisplayPortIn3 = "displayPortIn3";
        /// <summary>
        /// displayPortOut
        /// </summary>
		public const string DisplayPortOut = "displayPortOut";
        /// <summary>
        /// dmIn
        /// </summary>
		public const string DmIn = "dmIn";
        /// <summary>
        /// dmOut
        /// </summary>
		public const string DmOut = "dmOut";
        /// <summary>
        /// dviIn
        /// </summary>
		public const string DviIn = "dviIn";
        /// <summary>
        /// dviIn1
        /// </summary>
        public const string DviIn1 = "dviIn1";
        /// <summary>
        /// dviOut
        /// </summary>
        public const string DviOut = "dviOut";
        /// <summary>
        /// hdmiIn
        /// </summary>
		public const string HdmiIn = "hdmiIn";
        /// <summary>
        /// hdmiIn1
        /// </summary>
		public const string HdmiIn1 = "hdmiIn1";
        /// <summary>
        /// hdmiIn1PC
        /// </summary>
        public const string HdmiIn1PC = "hdmiIn1PC";
        /// <summary>
        /// hdmiIn2
        /// </summary>
		public const string HdmiIn2 = "hdmiIn2";
        /// <summary>
        /// hdmiIn2PC
        /// </summary>
        public const string HdmiIn2PC = "hdmiIn2PC";
        /// <summary>
        /// hdmiIn3
        /// </summary>
		public const string HdmiIn3 = "hdmiIn3";
        /// <summary>
        /// hdmiIn4
        /// </summary>
		public const string HdmiIn4 = "hdmiIn4";
        /// <summary>
        /// hdmiIn5
        /// </summary>
		public const string HdmiIn5 = "hdmiIn5";
        /// <summary>
        /// hdmiIn6
        /// </summary>
		public const string HdmiIn6 = "hdmiIn6";
        /// <summary>
        /// hdmiOut
        /// </summary>
		public const string HdmiOut = "hdmiOut";
        /// <summary>
        /// hdmiOut1
        /// </summary>
        public const string HdmiOut1 = "hdmiOut1";
        /// <summary>
        /// hdmiOut2
        /// </summary>
        public const string HdmiOut2 = "hdmiOut2";
        /// <summary>
        /// hdmiOut3
        /// </summary>
        public const string HdmiOut3 = "hdmiOut3";
        /// <summary>
        /// hdmiOut4
        /// </summary>
        public const string HdmiOut4 = "hdmiOut4";
        /// <summary>
        /// hdmiOut5
        /// </summary>
        public const string HdmiOut5 = "hdmiOut5";
        /// <summary>
        /// hdmiOut6
        /// </summary>
        public const string HdmiOut6 = "hdmiOut6";
        /// <summary>
        /// none
        /// </summary>
        public const string None = "none";
        /// <summary>
        /// rgbIn
        /// </summary>
		public const string RgbIn = "rgbIn";
        /// <summary>
        /// rgbIn1
        /// </summary>
        public const string RgbIn1 = "rgbIn1";
        /// <summary>
        /// rgbIn2
        /// </summary>
        public const string RgbIn2 = "rgbIn2";
        /// <summary>
        /// vgaIn
        /// </summary>
		public const string VgaIn = "vgaIn";
        /// <summary>
        /// vgaIn1
        /// </summary>
        public const string VgaIn1 = "vgaIn1";
        /// <summary>
        /// vgaOut
        /// </summary>
		public const string VgaOut = "vgaOut";
        /// <summary>
        /// IPC/OPS
        /// </summary>
        public const string IpcOps = "ipcOps";
        /// <summary>
        /// MediaPlayer
        /// </summary>
        public const string MediaPlayer = "mediaPlayer";
        /// <summary>
        /// UsbCIn
        /// </summary>
        public const string UsbCIn = "usbCIn";
        /// <summary>
        /// UsbCIn1
        /// </summary>
        public const string UsbCIn1 = "usbCIn1";
        /// <summary>
        /// UsbCIn2
        /// </summary>
        public const string UsbCIn2 = "usbCIn2";
        /// <summary>
        /// UsbCIn3
        /// </summary>
        public const string UsbCIn3 = "usbCIn3";
        /// <summary>
        /// UsbCOut
        /// </summary>
        public const string UsbCOut = "usbCOut";
        /// <summary>
        /// UsbCOut1
        /// </summary>
        public const string UsbCOut1 = "usbCOut1";
        /// <summary>
        /// UsbCOut2
        /// </summary>
        public const string UsbCOut2 = "usbCOut2";
        /// <summary>
        /// UsbCOut3
        /// </summary>
        public const string UsbCOut3 = "usbCOut3";
        /// <summary>
        /// HdBaseTIn
        /// </summary>
        public const string HdBaseTIn = "hdBaseTIn";
        /// <summary>
        /// HdBaseTOut
        /// </summary>
        public const string HdBaseTOut = "hdBaseTOut";
    }
}