using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Room.Cotija
{
	public interface IDelayedConfiguration
	{
		event EventHandler<EventArgs> ConfigurationIsReady;
	}
}