using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;

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
  /// <summary>
  /// GetOffsetJoin method
  /// </summary>
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

		public DefaultPageManager(IUiDisplayInfo device, BasicTriList trilist)
		{
			TriList = trilist;
			BackingPageJoin = GetOffsetJoin(device.DisplayUiType) + 1;
		}

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
		protected BasicTriListWithSmartObject TriList;
		protected uint LeftSubpageJoin;
		protected uint BackingPageJoin;
		protected uint[] AllLeftSubpages;
		protected uint DisplayUiType;

		protected MediumLeftSwitchablePageManager(uint displayUiType)
		{
			DisplayUiType = displayUiType;
		}

		protected void InterlockLeftSubpage(uint join)
		{
			join = join + GetOffsetJoin();
			ClearLeftInterlock();
			TriList.BooleanInput[join].BoolValue = true;
			LeftSubpageJoin = join;
		}

		protected void ClearLeftInterlock()
		{
			foreach (var p in AllLeftSubpages)
				TriList.BooleanInput[GetOffsetJoin() + p].BoolValue = false;
		}

		protected uint GetOffsetJoin()
		{
			return GetOffsetJoin(DisplayUiType);
		}
	}
}