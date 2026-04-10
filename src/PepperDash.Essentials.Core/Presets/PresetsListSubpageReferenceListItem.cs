using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core.Presets
{
 /// <summary>
 /// Represents a PresetsListSubpageReferenceListItem
 /// </summary>
	public class PresetsListSubpageReferenceListItem : SubpageReferenceListItem
	{
		DevicePresetsView View;
		PresetChannel Channel;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chan">preset channel</param>
		/// <param name="index">index of the preset</param>
		/// <param name="owner">owner of the subpage reference list item</param>
		/// <param name="view">device presets view</param>
		public PresetsListSubpageReferenceListItem(PresetChannel chan, uint index, 
			SubpageReferenceList owner, DevicePresetsView view)
			: base(index, owner)
		{
			View = view;
			Channel = chan;
			owner.GetBoolFeedbackSig(index, 1).UserObject = new Action<bool>(b => { if (!b) view.Model.Dial((int)index); });
			Refresh();
		}

		/// <summary>
		/// Clear method
		/// </summary>
		/// <inheritdoc />
		public override void Clear()
		{
			Owner.GetBoolFeedbackSig(Index, 1).UserObject = null;
			Owner.StringInputSig(Index, 1).StringValue = "";
			Owner.StringInputSig(Index, 2).StringValue = "";
			Owner.StringInputSig(Index, 3).StringValue = "";
		}

		/// <summary>
		/// Refresh method
		/// </summary>
		/// <inheritdoc />
		public override void Refresh()
		{
			var name = View.ShowName ? Channel.Name : "";
			Owner.StringInputSig(Index, 1).StringValue = name;
			var chan = View.ShowNumbers ? Channel.Channel : "";
			Owner.StringInputSig(Index, 2).StringValue = chan;
			var url = View.Model.ImagesLocalHostPrefix + View.Model.ImagesPathPrefix + Channel.IconUrl;
			Debug.LogMessage(LogEventLevel.Verbose, "icon url={0}", url);
			var icon = View.ShowIcon ? url : "";
			Owner.StringInputSig(Index, 3).StringValue = icon;
		}
	}
}