using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

namespace PepperDash.Essentials.Core
{
	public class SubpageReferenceListItem
	{
		/// <summary>
		/// The list that this lives in
		/// </summary>
		protected SubpageReferenceList Owner;
		protected uint Index;

		public SubpageReferenceListItem(uint index, SubpageReferenceList owner)
		{
			Index = index;
			Owner = owner;
		}

		/// <summary>
		/// Called by SRL to release all referenced objects
		/// </summary>
		public virtual void Clear()
		{
		}

		public virtual void Refresh() { }
	}
}