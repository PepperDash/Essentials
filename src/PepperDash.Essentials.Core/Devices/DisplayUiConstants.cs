using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// Integers that represent the "source type number" for given sources.
	/// Primarily used by the UI to calculate subpage join offsets
	/// Note, for UI, only values 1-49 are valid.
	/// </summary>
	public class DisplayUiConstants
	{
		public const uint TypeRadio = 1;
		public const uint TypeDirecTv = 9;
		public const uint TypeBluray = 13;
		public const uint TypeChromeTv = 15;
		public const uint TypeFireTv = 16;
		public const uint TypeAppleTv = 17;
		public const uint TypeRoku = 18;
		public const uint TypeLaptop = 31;
		public const uint TypePc = 32;

		public const uint TypeNoControls = 49;
	}