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
	public class SingleSubpageModalAndBackDriver : PanelDriverBase
	{
		BoolInputSig SubpageSig;

		PanelDriverBase Parent;

		public SingleSubpageModalAndBackDriver(PanelDriverBase parent, uint subpageJoin) : base(parent.TriList)
		{
			Parent = parent;
			SubpageSig = Parent.TriList.BooleanInput[subpageJoin];
		}

		/// <summary>
		/// This shows the driver.
		/// Not sure I like this approach.  Hides this and shows it's parent.  Not really a navigation-stack type thing.
		/// The parent is always the home page driver
		/// </summary>
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

		public override void BackButtonPressed()
		{
			Hide();
			Parent.Show();
		}
	}
}