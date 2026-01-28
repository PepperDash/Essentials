using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Represents a SubpageReferenceListItem
 /// </summary>
	public class SubpageReferenceListItem
	{
		/// <summary>
		/// The list that this lives in
		/// </summary>
		protected SubpageReferenceList Owner;

		/// <summary>
		/// The index of this item
		/// </summary>
		protected uint Index;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="index">index of the item</param>
		/// <param name="owner">owner of the item</param>
		public SubpageReferenceListItem(uint index, SubpageReferenceList owner)
		{
			Index = index;
			Owner = owner;
		}

  /// <summary>
  /// Clear method
  /// </summary>
  /// <inheritdoc />
		public virtual void Clear()
		{
		}

  /// <summary>
  /// Refresh method
  /// </summary>
		public virtual void Refresh() { }
	}
}