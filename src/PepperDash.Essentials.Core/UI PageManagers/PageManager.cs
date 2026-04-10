using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.PageManagers
{
	/// <summary>
	/// The PageManager classes are used to bridge a device to subpage
	/// visibility.
	/// </summary>
	public abstract class PageManager
	{
		/// <summary>
		/// ActiveJoins list
		/// </summary>
		protected List<uint> ActiveJoins = new List<uint>();

		/// <summary>
		/// Show method
		/// </summary>
		public abstract void Show();

		/// <summary>
		/// Hide method
		/// </summary>
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

	/// <summary>
	/// A simple class that hides and shows the default subpage for a given source type
	/// </summary>
	public class DefaultPageManager : PageManager
	{
		BasicTriList TriList;
		uint BackingPageJoin;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="device">device</param>
		/// <param name="trilist">trilist object</param>
		public DefaultPageManager(IUiDisplayInfo device, BasicTriList trilist)
		{
			TriList = trilist;
			BackingPageJoin = GetOffsetJoin(device.DisplayUiType) + 1;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="join">back page join</param>
		/// <param name="trilist">trilist object</param>
		public DefaultPageManager(uint join, BasicTriList trilist)
		{
			TriList = trilist;
			BackingPageJoin = join;
		}

		/// <summary>
		/// Show method
		/// </summary>
		/// <inheritdoc />
		public override void Show()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = true;
		}

		/// <summary>
		/// Hide method
		/// </summary>
		public override void Hide()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = false;
		}
	}

	/// <summary>
	/// A page manager for a page with backing panel and a switchable side panel
	/// </summary>
	public abstract class MediumLeftSwitchablePageManager : PageManager
	{
		/// <summary>
		/// TriList object
		/// </summary>
		protected BasicTriListWithSmartObject TriList;

		/// <summary>
		/// Left subpage join
		/// </summary>
		protected uint LeftSubpageJoin;

		/// <summary>
		/// Backing page join
		/// </summary>
		protected uint BackingPageJoin;

		/// <summary>
		/// All left subpages
		/// </summary>
		protected uint[] AllLeftSubpages;

		/// <summary>
		/// Display UI Type
		/// </summary>
		protected uint DisplayUiType;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="displayUiType">UI type</param>
		protected MediumLeftSwitchablePageManager(uint displayUiType)
		{
			DisplayUiType = displayUiType;
		}

		/// <summary>
		/// Interlock left subpage
		/// </summary>
		/// <param name="join"></param>
		protected void InterlockLeftSubpage(uint join)
		{
			join = join + GetOffsetJoin();
			ClearLeftInterlock();
			TriList.BooleanInput[join].BoolValue = true;
			LeftSubpageJoin = join;
		}

		/// <summary>
		/// Clear left interlock
		/// </summary>
		protected void ClearLeftInterlock()
		{
			foreach (var p in AllLeftSubpages)
				TriList.BooleanInput[GetOffsetJoin() + p].BoolValue = false;
		}

		/// <summary>
		/// Get offset join
		/// </summary>
		/// <returns></returns>
		protected uint GetOffsetJoin()
		{
			return GetOffsetJoin(DisplayUiType);
		}
	}
}