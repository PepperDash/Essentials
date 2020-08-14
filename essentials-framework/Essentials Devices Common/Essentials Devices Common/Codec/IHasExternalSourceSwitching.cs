using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
namespace PepperDash.Essentials.Devices.Common.Codec
{
	public interface IHasExternalSourceSwitching
	{
		bool ExternalSourceListEnabled { get; }
		void AddExternalSource(string connectorId, string name);
		void ClearExternalSources();
		Action<string, string> RunRouteAction { set;}
	}

}