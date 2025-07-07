using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Presets;

	public class DevicePresetsView
	{
		public bool ShowNumbers { get; set; }
		public bool ShowName { get; set; }
		public bool ShowIcon { get; set; }

    public SubpageReferenceList SRL { get; private set; }
		public DevicePresetsModel Model { get; private set; }

		public DevicePresetsView(BasicTriListWithSmartObject tl, DevicePresetsModel model)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model", "DevicePresetsView Cannot be instantiated with null model");
			}
			ShowIcon = true;
			ShowName = true;

			Model = model;

			SRL = new SubpageReferenceList(tl, 10012, 3, 0, 4);
			Model.PresetsLoaded += new EventHandler(Model_PresetsLoaded);
		}

		public void Attach()
		{
			if (Model.PresetsAreLoaded)
			{
				uint index = 1;
				foreach (var p in Model.PresetsList)
				{
					SRL.AddItem(new PresetsListSubpageReferenceListItem(p, index, SRL, this));
					index++;
				}
				SRL.Count = (ushort)Model.PresetsList.Count;
			}
		}

		public void Detach()
		{
			SRL.Clear();
		}

		void Model_PresetsLoaded(object sender, EventArgs e)
		{
			Detach();
			Attach();
		}
	}