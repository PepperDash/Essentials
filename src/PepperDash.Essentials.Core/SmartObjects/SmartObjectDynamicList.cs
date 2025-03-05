using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core.SmartObjects
{
	public class SmartObjectDynamicList : SmartObjectHelperBase
	{
		public const string SigNameScrollToItem = "Scroll To Item";
		public const string SigNameSetNumberOfItems = "Set Number of Items";

		public uint NameSigOffset { get; private set; }

		public ushort Count 
		{
			get 
			{	
				return SmartObject.UShortInput[SigNameSetNumberOfItems].UShortValue; 
			}
			set { SmartObject.UShortInput[SigNameSetNumberOfItems].UShortValue = value; }
		}

		/// <summary>
		/// The limit of the list object, as defined by VTPro settings.  Zero if object
		/// is not a list
		/// </summary>
		public int MaxCount { get; private set; }

        /// <summary>
        /// Wrapper for smart object
        /// </summary>
        /// <param name="so"></param>
        /// <param name="useUserObjectHandler">True if the standard user object action handler will be used</param>
        /// <param name="nameSigOffset">The starting join of the string sigs for the button labels</param>
		public SmartObjectDynamicList(SmartObject so, bool useUserObjectHandler, uint nameSigOffset) : base(so, useUserObjectHandler) 
		{		
			try
			{
				// Just try to touch the count signal to make sure this is indeed a dynamic list
				var c = Count;
				NameSigOffset = nameSigOffset;
				MaxCount = SmartObject.BooleanOutput.Count(s => s.Name.EndsWith("Pressed"));
				//Debug.LogMessage(LogEventLevel.Verbose, "Smart object {0} has {1} max", so.ID, MaxCount);
			}
			catch
			{
				var msg = string.Format("SmartObjectDynamicList: Smart Object {0:X2}-{1} is not a dynamic list. Ignoring", so.Device.ID, so.ID);
				Debug.LogMessage(LogEventLevel.Information, msg);
			}
		}

		/// <summary>
		/// Builds a new list item
		/// </summary>
		public void SetItem(uint index, string mainText, string iconName, Action<bool> action)
		{
			SetItemMainText(index, mainText);
			SetItemIcon(index, iconName);
			SetItemButtonAction(index, action);
			//try
			//{
			//    SetMainButtonText(index, text);
			//    SetIcon(index, iconName);
			//    SetButtonAction(index, action);
			//}
			//catch(Exception e)
			//{
			//    Debug.LogMessage(LogEventLevel.Debug, "Cannot set Dynamic List item {0} on smart object {1}", index, SmartObject.ID);
			//    ErrorLog.Warn(e.ToString());
			//}
		}

		public void SetItemMainText(uint index, string text)
		{
			if (index > MaxCount) return;
			// The list item template defines CIPS tags that refer to standard joins
			(SmartObject.Device as BasicTriList).StringInput[NameSigOffset + index].StringValue = text;
		}

		public void SetItemIcon(uint index, string iconName)
		{
			if (index > MaxCount) return;
			SmartObject.StringInput[string.Format("Set Item {0} Icon Serial", index)].StringValue = iconName;
		}

		public void SetItemButtonAction(uint index, Action<bool> action)
		{
			if (index > MaxCount) return;
			SmartObject.BooleanOutput[string.Format("Item {0} Pressed", index)].UserObject = action;
		}

		/// <summary>
		/// Sets the feedback on the given line, clearing others when interlocked is set
		/// </summary>
		public void SetFeedback(uint index, bool interlocked)
		{
			if (interlocked) 
				ClearFeedbacks();
			SmartObject.BooleanInput[string.Format("Item {0} Selected", index)].BoolValue = true;
		}

		/// <summary>
		/// Clears all button feedbacks
		/// </summary>
		public void ClearFeedbacks()
		{
			for(int i = 1; i<= Count; i++)
				SmartObject.BooleanInput[string.Format("Item {0} Selected", i)].BoolValue = false;
		}

		/// <summary>
		/// Removes Action object from all buttons
		/// </summary>
		public void ClearActions()
		{
			Debug.LogMessage(LogEventLevel.Verbose, "SO CLEAR");
			for(ushort i = 1; i <= MaxCount; i++)
				SmartObject.BooleanOutput[string.Format("Item {0} Pressed", i)].UserObject = null;
		}
	}
}