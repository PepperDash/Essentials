extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Linq;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface IComPortsDevice
	{
		IComPorts Device { get; }
	}
}