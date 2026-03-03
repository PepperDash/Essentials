using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDvr : IDPad
	{
		/// <summary>
		/// DVR List button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void DvrList(bool pressRelease);

		/// <summary>
		/// Record button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Record(bool pressRelease);
	}

	/// <summary>
	/// IDvrExtensions class
	/// </summary>
	public static class IDvrExtensions
	{
		/// <summary>
		/// LinkButtons method
		/// </summary>
		/// <param name="dev">IDvr device</param>
		/// <param name="triList">BasicTriList to link to</param>
		public static void LinkButtons(this IDvr dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(136, dev.DvrList);
			triList.SetBoolSigAction(152, dev.Record);
		}

		/// <summary>
		/// UnlinkButtons method
		/// </summary>
		/// <param name="dev">IDvr device</param>
		/// <param name="triList">BasicTriList to unlink from</param>
		public static void UnlinkButtons(this IDvr dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(136);
			triList.ClearBoolSigAction(152);
		}
	}
}