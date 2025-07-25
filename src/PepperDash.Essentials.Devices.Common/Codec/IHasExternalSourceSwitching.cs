using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec.Cisco;

namespace PepperDash.Essentials.Devices.Common.Codec
{
 /// <summary>
 /// Defines the contract for IHasExternalSourceSwitching
 /// </summary>
	public interface IHasExternalSourceSwitching
	{
		bool ExternalSourceListEnabled { get; }
        string ExternalSourceInputPort { get; }
		void AddExternalSource(string connectorId, string key, string name, eExternalSourceType type);
		void SetExternalSourceState(string key, eExternalSourceMode mode);
		void ClearExternalSources();
        void SetSelectedSource(string key);
		Action<string, string> RunRouteAction { set;}
	}

}