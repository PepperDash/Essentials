using System;
using System.Collections.Generic;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
	public enum eAvSubpageType
	{
		NoControls, 
		PowerOff, 
		SetupFullDistributed, 
		SourceWaitOverlay, 
		TopBar, 
		VolumePopup, 
		ZoneSource
	}

	public enum eAvSourceSubpageType
	{
		AppleTv,
		Radio,
		Roku
	}

	public enum eCommonSubpageType
	{
		GenericModal,
		Home,
		PanelSetup,
		Weather
	}

	public enum eAvSmartObjects
	{
		RoomList,
		SourceList
	}

	public enum eCommonSmartObjects
	{
		HomePageList
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class PanelDriverBase// : IBasicTriListWithSmartObject
	{
		/// <summary>
		/// 
		/// </summary>
		public bool IsVisible { get; private set; }

		/// <summary>
		/// Makes sure you call this. 
		/// Sets IsVisible and attaches back/home buttons to BackButtonPressed
		/// </summary>
		public virtual void Show() 
		{
			IsVisible = true;
			TriList.SetSigFalseAction(15002, BackButtonPressed);
		}

		/// <summary>
		/// Only sets IsVisible
		/// </summary>
		public virtual void Hide() 
		{
			IsVisible = false;
		}

		/// <summary>
		/// Override with specific back button behavior. Default is empty
		/// </summary>
		public virtual void BackButtonPressed()
		{
		}

		public PanelDriverBase(Crestron.SimplSharpPro.DeviceSupport.BasicTriListWithSmartObject triList)
		{
			TriList = triList;
		}

		#region IBasicTriListWithSmartObject Members

		/// <summary>
		/// 
		/// </summary>
		public void AddSmartObjectHelper(uint id, object controller)
		{
			SmartObjectControllers.Add(id, controller);
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveSmartObjectHelper(uint id)
		{
			SmartObjectControllers.Remove(id);
		}

		Dictionary<uint, object> SmartObjectControllers = new Dictionary<uint, object>();

		/// <summary>
		/// The trilist object for the Crestron TP device
		/// </summary>
		public Crestron.SimplSharpPro.DeviceSupport.BasicTriListWithSmartObject TriList { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool ContainsSmartObjectHelper(uint id)
		{
			return SmartObjectControllers.ContainsKey(id);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public object GetSmartObjectHelper(uint id)
		{
			if (SmartObjectControllers.ContainsKey(id))
				return SmartObjectControllers[id];
			else
				return null;
		}

		#endregion
	}
}