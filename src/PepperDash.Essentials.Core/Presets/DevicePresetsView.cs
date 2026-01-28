using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Presets
{
	/// <summary>
	/// Represents a DevicePresetsView
	/// </summary>
	public class DevicePresetsView
	{
		/// <summary>
		/// Gets or sets the ShowNumbers
		/// </summary>
		public bool ShowNumbers { get; set; }

		/// <summary>
		/// Gets or sets the ShowName
		/// </summary>
		public bool ShowName { get; set; }

		/// <summary>
		/// Gets or sets the ShowIcon
		/// </summary>
		public bool ShowIcon { get; set; }

        /// <summary>
        /// Gets or sets the SRL
        /// </summary>
        public SubpageReferenceList SRL { get; private set; }

		/// <summary>
		/// Gets or sets the Model
		/// </summary>
		public DevicePresetsModel Model { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tl">trilst</param>
		/// <param name="model">device presets model</param>
		/// <exception cref="ArgumentNullException"></exception>
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

		/// <summary>
		/// Attach method
		/// </summary>
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

		/// <summary>
		/// Detach method
		/// </summary>
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
}