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
		public const string AntennaIn = "antennaIn";
		public const string AnyAudioIn = "anyAudioIn";
		public const string AnyAudioOut = "anyAudioOut";
        public const string AnyOut = "anyOut";
        public const string AnyVideoIn = "anyVideoIn";
		public const string AnyVideoOut = "anyVideoOut";
		public const string BalancedAudioOut = "balancedAudioOut";
		public const string ComponentIn = "componentIn";
		public const string ComponentOut = "componentOut";
		public const string CompositeIn = "compositeIn";
		public const string CompositeOut = "compositeOut";
		public const string DisplayPortIn = "displayPortIn";
		public const string DisplayPortIn1 = "displayPortIn1";
		public const string DisplayPortIn2 = "displayPortIn2";
		public const string DisplayPortIn3 = "displayPortIn3";
		public const string DisplayPortOut = "displayPortOut";
		public const string DmIn = "dmIn";
		public const string DmOut = "dmOut";
		public const string DviIn = "dviIn";
        public const string DviIn1 = "dviIn1";
        public const string DviOut = "dviOut";
		public const string HdmiIn = "hdmiIn";
		public const string HdmiIn1 = "hdmiIn1";
		public const string HdmiIn2 = "hdmiIn2";
		public const string HdmiIn3 = "hdmiIn3";
		public const string HdmiIn4 = "hdmiIn4";
		public const string HdmiIn5 = "hdmiIn5";
		public const string HdmiIn6 = "hdmiIn6";
		public const string HdmiOut = "hdmiOut";
		public const string RgbIn = "rgbIn";
        public const string RgbIn1 = "rgbIn1";
        public const string RgbIn2 = "rgbIn2";
		public const string VgaIn = "vgaIn";
		public const string VgaOut = "vgaOut";
	}
}