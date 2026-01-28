using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Integers that represent the "source type number" for given sources.
	/// Primarily used by the UI to calculate subpage join offsets
	/// Note, for UI, only values 1-49 are valid.
	/// </summary>
	public class DisplayUiConstants
	{
		/// <summary>
		/// TypeRadio constant
		/// </summary>
		public const uint TypeRadio = 1;

		/// <summary>
		/// TypeTv constant
		/// </summary>
		public const uint TypeDirecTv = 9;

		/// <summary>
		/// TypeBluray constant
		/// </summary>
		public const uint TypeBluray = 13;

		/// <summary>
		/// TypeStreamingDevice constant
		/// </summary>
		public const uint TypeChromeTv = 15;

		/// <summary>
		/// TypeStreamingDevice constant
		/// </summary>
		public const uint TypeFireTv = 16;

		/// <summary>
		/// TypeStreamingDevice constant
		/// </summary>
		public const uint TypeAppleTv = 17;

		/// <summary>
		/// TypeStreamingDevice constant
		/// </summary>
		public const uint TypeRoku = 18;

		/// <summary>
		/// TypeLaptop constant
		/// </summary>
		public const uint TypeLaptop = 31;

		/// <summary>
		/// TypePc constant
		/// </summary>
		public const uint TypePc = 32;

		/// <summary>
		/// TypeNoControls constant
		/// </summary>
		public const uint TypeNoControls = 49;
	}
}