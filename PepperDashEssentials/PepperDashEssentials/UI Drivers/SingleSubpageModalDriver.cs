using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;


namespace PepperDash.Essentials
{
	/// <summary>
	/// Very basic show/hide manager for weather page.  Basic functionality is useful on any 
	/// size of interface
	/// </summary>
	public class SingleSubpageModalDriver : PanelDriverBase
	{
		BoolInputSig SubpageSig;

        public SingleSubpageModalDriver(PanelDriverBase parent, uint subpageJoin, uint closeJoin)
            : base(parent.TriList)
		{
			SubpageSig = parent.TriList.BooleanInput[subpageJoin];
            parent.TriList.SetSigFalseAction(closeJoin, Hide);
		}

		public override void Show()
		{
			SubpageSig.BoolValue = true;
			base.Show();
		}

		public override void Hide()
		{
			SubpageSig.BoolValue = false;
			base.Hide();
		}
	}
}