
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
	////*****************************************************************************
	///// <summary>
	///// Base class for all subpage reference list controllers
	///// </summary>
	//public class SubpageReferenceListController
	//{
	//    public SubpageReferenceList TheList { get; protected set; }
	//}

	//*****************************************************************************
	/// <summary>
	/// Wrapper class for subpage reference list.  Contains helpful methods to get at the various signal groupings
	/// and to get individual signals using an index and a join.
	/// </summary>
	public class SubpageReferenceList
	{
		/// <summary>
		/// Gets or sets the Count
		/// </summary>
		public ushort Count
		{
			get { return SetNumberOfItemsSig.UShortValue; }
			set { SetNumberOfItemsSig.UShortValue = value; }
		}

		/// <summary>
		/// Gets or sets the MaxDefinedItems
		/// </summary>
		public ushort MaxDefinedItems { get; private set; }

		/// <summary>
		/// Gets or sets the ScrollToItemSig
		/// </summary>
		public UShortInputSig ScrollToItemSig { get; private set; }

		UShortInputSig SetNumberOfItemsSig;

		/// <summary>
		/// Gets or sets the BoolIncrement
		/// </summary>
		public uint BoolIncrement { get; protected set; }

		/// <summary>
		/// Gets or sets the UShortIncrement
		/// </summary>
		public uint UShortIncrement { get; protected set; }

		/// <summary>
		/// Gets or sets the StringIncrement
		/// </summary>
		public uint StringIncrement { get; protected set; }

		/// <summary>
		/// Gets or sets the SRL
		/// </summary>
		protected readonly SmartObject SRL;

		/// <summary>
		/// Gets the list of items in the SRL
		/// </summary>
		protected readonly List<SubpageReferenceListItem> Items = new List<SubpageReferenceListItem>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="triList">trilist for the smart object</param>
		/// <param name="smartObjectId">smart object ID</param>
		/// <param name="boolIncrement"></param>
		/// <param name="ushortIncrement"></param>
		/// <param name="stringIncrement"></param>
		public SubpageReferenceList(BasicTriListWithSmartObject triList, uint smartObjectId,
			uint boolIncrement, uint ushortIncrement, uint stringIncrement)
		{
			SmartObject obj;
			// Fail cleanly if not defined
			if (triList.SmartObjects == null || triList.SmartObjects.Count == 0)
			{
				Debug.LogMessage(LogEventLevel.Information, "TriList {0:X2} Smart objects have not been loaded", triList.ID, smartObjectId);
				return;
			}
			if (triList.SmartObjects.TryGetValue(smartObjectId, out obj))
			{
				SRL = triList.SmartObjects[smartObjectId];
				ScrollToItemSig = SRL.UShortInput["Scroll To Item"];
				SetNumberOfItemsSig = SRL.UShortInput["Set Number of Items"];
				BoolIncrement = boolIncrement;
				UShortIncrement = ushortIncrement;
				StringIncrement = stringIncrement;

				// Count the enable lines to see what max items is
				MaxDefinedItems = (ushort)SRL.BooleanInput
					.Where(s => s.Name.Contains("Enable")).Count();
				Debug.LogMessage(LogEventLevel.Verbose, "SRL {0} contains max {1} items", SRL.ID, MaxDefinedItems);

				SRL.SigChange -= new SmartObjectSigChangeEventHandler(SRL_SigChange);
				SRL.SigChange += new SmartObjectSigChangeEventHandler(SRL_SigChange);
			}
			else
				Debug.LogMessage(LogEventLevel.Information, "ERROR: TriList 0x{0:X2} Cannot load smart object {1}. Verify correct SGD file is loaded",
										triList.ID, smartObjectId);
		}

		/// <summary>
		/// Adds item to saved list of displayed items (not necessarily in order)
		/// DOES NOT adjust Count
		/// </summary>
		/// <param name="item"></param>
		/// <summary>
		/// AddItem method
		/// </summary>
		public void AddItem(SubpageReferenceListItem item)
		{
			Items.Add(item);
		}

		/// <summary>
		/// Clear method
		/// </summary>
		public void Clear()
		{
			// If a line item needs to disconnect an CueActionPair or do something to release RAM
			foreach (var item in Items) item.Clear();
			// Empty the list
			Items.Clear();
			// Clean up the SRL
			Count = 1;

			ScrollToItemSig.UShortValue = 1;
		}

		/// <summary>
		/// Refresh method
		/// </summary>
		public void Refresh()
		{
			foreach (var item in Items) item.Refresh();
		}


		// Helpers to get sigs by their weird SO names

		/// <summary>
		/// Returns the Sig associated with a given SRL line index
		/// and the join number of the object on the SRL subpage.
		/// Note: If the join number exceeds the increment range, or the count of Sigs on the 
		/// list object, this will return null
		/// </summary>
		/// <param name="index">The line or item position on the SRL</param>
		/// <param name="sigNum">The join number of the item on the SRL subpage</param>
		/// <returns>A Sig or null if the numbers are out of range</returns>
		public BoolOutputSig GetBoolFeedbackSig(uint index, uint sigNum)
		{
			if (sigNum > BoolIncrement) return null;
			return SRL.BooleanOutput.FirstOrDefault(s => s.Name.Equals(GetBoolFeedbackSigName(index, sigNum)));
		}

		/// <summary>
		/// Returns the Sig associated with a given SRL line index
		/// and the join number of the object on the SRL subpage.
		/// Note: If the join number exceeds the increment range, or the count of Sigs on the 
		/// list object, this will return null
		/// </summary>
		/// <param name="index">The line or item position on the SRL</param>
		/// <param name="sigNum">The join number of the item on the SRL subpage</param>
		/// <returns>A Sig or null if the numbers are out of range</returns>
		public UShortOutputSig GetUShortOutputSig(uint index, uint sigNum)
		{
			if (sigNum > UShortIncrement) return null;
			return SRL.UShortOutput.FirstOrDefault(s => s.Name.Equals(GetUShortOutputSigName(index, sigNum)));
		}

		/// <summary>
		/// Returns the Sig associated with a given SRL line index
		/// and the join number of the object on the SRL subpage.
		/// Note: If the join number exceeds the increment range, or the count of Sigs on the 
		/// list object, this will return null
		/// </summary>
		/// <param name="index">The line or item position on the SRL</param>
		/// <param name="sigNum">The join number of the item on the SRL subpage</param>
		/// <returns>A Sig or null if the numbers are out of range</returns>
		public StringOutputSig GetStringOutputSig(uint index, uint sigNum)
		{
			if (sigNum > StringIncrement) return null;
			return SRL.StringOutput.FirstOrDefault(s => s.Name.Equals(GetStringOutputSigName(index, sigNum)));
		}

		/// <summary>
		/// Returns the Sig associated with a given SRL line index
		/// and the join number of the object on the SRL subpage.
		/// Note: If the join number exceeds the increment range, or the count of Sigs on the 
		/// list object, this will return null
		/// </summary>
		/// <param name="index">The line on the SRL</param>
		/// <param name="sigNum">The join number of the item on the SRL subpage</param>
		/// <returns>A Sig or null if the numbers are out of range</returns>
		public BoolInputSig BoolInputSig(uint index, uint sigNum)
		{
			if (sigNum > BoolIncrement) return null;
			return SRL.BooleanInput.FirstOrDefault(s => s.Name.Equals(GetBoolInputSigName(index, sigNum)));
		}

		/// <summary>
		/// Returns the Sig associated with a given SRL line index
		/// and the join number of the object on the SRL subpage.
		/// Note: If the join number exceeds the increment range, or the count of Sigs on the 
		/// list object, this will return null
		/// </summary>
		/// <param name="index">The line on the SRL</param>
		/// <param name="sigNum">The join number of the item on the SRL subpage</param>
		/// <returns>A Sig or null if the numbers are out of range</returns>
		public UShortInputSig UShortInputSig(uint index, uint sigNum)
		{
			if (sigNum > UShortIncrement) return null;
			return SRL.UShortInput.FirstOrDefault(s => s.Name.Equals(GetUShortInputSigName(index, sigNum)));
		}

		/// <summary>
		/// Returns the Sig associated with a given SRL line index
		/// and the join number of the object on the SRL subpage.
		/// Note: If the join number exceeds the increment range, or the count of Sigs on the 
		/// list object, this will return null
		/// </summary>
		/// <param name="index">The line on the SRL</param>
		/// <param name="sigNum">The join number of the item on the SRL subpage</param>
		/// <returns>A Sig or null if the numbers are out of range</returns>
		public StringInputSig StringInputSig(uint index, uint sigNum)
		{
			if (sigNum > StringIncrement) return null;
			return SRL.StringInput.FirstOrDefault(s => s.Name.Equals(GetStringInputSigName(index, sigNum)));
		}

		// Helpers to get signal names

		string GetBoolFeedbackSigName(uint index, uint sigNum)
		{
			var num = (index - 1) * BoolIncrement + sigNum;
			return String.Format("press{0}", num);
		}

		string GetUShortOutputSigName(uint index, uint sigNum)
		{
			var num = (index - 1) * UShortIncrement + sigNum;
			return String.Format("an_act{0}", num);
		}

		string GetStringOutputSigName(uint index, uint sigNum)
		{
			var num = (index - 1) * StringIncrement + sigNum;
			return String.Format("text-i{0}", num);
		}

		string GetBoolInputSigName(uint index, uint sigNum)
		{
			var num = (index - 1) * BoolIncrement + sigNum;
			return String.Format("fb{0}", num);
		}

		string GetUShortInputSigName(uint index, uint sigNum)
		{
			var num = (index - 1) * UShortIncrement + sigNum;
			return String.Format("an_fb{0}", num);
		}

		string GetStringInputSigName(uint index, uint sigNum)
		{
			var num = (index - 1) * StringIncrement + sigNum;
			return String.Format("text-o{0}", num);
		}

		/// <summary>
		/// Stock SigChange handler
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		/// <summary>
		/// SRL_SigChange method
		/// </summary>
		public static void SRL_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
		{
			var uo = args.Sig.UserObject;
			if (uo is Action<bool>)
				(uo as Action<bool>)(args.Sig.BoolValue);
			else if (uo is Action<ushort>)
				(uo as Action<ushort>)(args.Sig.UShortValue);
			else if (uo is Action<string>)
				(uo as Action<string>)(args.Sig.StringValue);
		}
	}
}