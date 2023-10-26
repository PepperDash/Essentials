using System.Collections.Generic;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.PageManagers
{
	/// <summary>
	/// The PageManager classes are used to bridge a device to subpage
	/// visibility.
	/// </summary>
	public abstract class PageManager
	{
		protected List<uint> ActiveJoins = new List<uint>();

		public abstract void Show();

		public abstract void Hide();

		/// <summary>
		/// For device types 1-49, returns the offset join for subpage management 10100 - 14900
		/// </summary>
		/// <param name="deviceType">1 through 49, as defined in some constants somewhere!</param>
		/// <returns></returns>
		public uint GetOffsetJoin(uint deviceType)
		{
			return 10000 + (deviceType * 100);
		}
	}
}