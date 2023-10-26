using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
	/// 
	/// </summary>
	public abstract class PanelDriverBase
	{
		/// <summary>
		/// 
		/// </summary>
		public bool IsVisible { get; private set; }

        public bool WasVisibleWhenHidden { get; private set; }

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
        /// Will show if this was visible when Hide was called (for group hiding/showing)
        /// </summary>
        public void Restore()
        {
            if (WasVisibleWhenHidden)
                Show();
        }

		/// <summary>
		/// Only sets IsVisible
		/// </summary>
		public virtual void Hide() 
		{
            WasVisibleWhenHidden = IsVisible;
			IsVisible = false;
		}

        /// <summary>
        /// Toggles visibility of this driver
        /// </summary>
        public virtual void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

		/// <summary>
		/// Override with specific back button behavior. Default is empty
		/// </summary>
		public virtual void BackButtonPressed()
		{
		}

		public PanelDriverBase(BasicTriListWithSmartObject triList)
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
		public BasicTriListWithSmartObject TriList { get; private set; }

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