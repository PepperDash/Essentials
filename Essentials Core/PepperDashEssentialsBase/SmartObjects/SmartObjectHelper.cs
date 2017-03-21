using System;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public class SmartObjectHelper
	{
		public static uint GetSmartObjectJoinForTypeAndObject(uint sourceType, uint typeOffset)
		{
			return (uint)(10000 + (sourceType * 100) + typeOffset);
		}

		//public static void DumpSmartObject(SmartGraphicsTouchpanelControllerBase tp, uint id)
		//{
		//    if (!tp.TriList.SmartObjects.Contains(id))
		//    {
		//        Debug.Console(0, tp, "does not contain smart object ID {0}", id);
		//        return;
		//    }
		//    var so = tp.TriList.SmartObjects[id];
		//    Debug.Console(0, tp, "Signals for smart object ID {0}", id);
		//    Debug.Console(0, "BooleanInput -------------------------------");
		//    foreach (var s in so.BooleanInput)
		//        Debug.Console(0, "  {0,5} {1}", s.Number, s.Name);
		//    Debug.Console(0, "UShortInput -------------------------------");
		//    foreach (var s in so.UShortInput)
		//        Debug.Console(0, "  {0,5} {1}", s.Number, s.Name);
		//    Debug.Console(0, "StringInput -------------------------------");
		//    foreach (var s in so.StringInput)
		//        Debug.Console(0, "  {0,5} {1}", s.Number, s.Name);
		//    Debug.Console(0, "BooleanOutput -------------------------------");
		//    foreach (var s in so.BooleanOutput)
		//        Debug.Console(0, "  {0,5} {1}", s.Number, s.Name);
		//    Debug.Console(0, "UShortOutput -------------------------------");
		//    foreach (var s in so.UShortOutput)
		//        Debug.Console(0, "  {0,5} {1}", s.Number, s.Name);
		//    Debug.Console(0, "StringOutput -------------------------------");
		//    foreach (var s in so.StringOutput)
		//        Debug.Console(0, "  {0,5} {1}", s.Number, s.Name);
		//}
	
		///// <summary>
		///// Inserts/removes the appropriate UO's onto sigs 
		///// </summary>
		///// <param name="triList"></param>
		///// <param name="smartObjectId"></param>
		///// <param name="deviceUserObjects"></param>
		///// <param name="state"></param>
		//public static void LinkNumpadWithUserObjects(BasicTriListWithSmartObject triList,
		//    uint smartObjectId, List<CueActionPair> deviceUserObjects, Cue Misc_1Function, Cue Misc_2Function, bool state)
		//{
		//    var sigDict = new Dictionary<string, Cue>
		//    {
		//        { "0", CommonBoolCue.Digit0 },
		//        { "1", CommonBoolCue.Digit1 },
		//        { "2", CommonBoolCue.Digit2 },
		//        { "3", CommonBoolCue.Digit3 },
		//        { "4", CommonBoolCue.Digit4 },
		//        { "5", CommonBoolCue.Digit5 },
		//        { "6", CommonBoolCue.Digit6 },
		//        { "7", CommonBoolCue.Digit7 },
		//        { "8", CommonBoolCue.Digit8 },
		//        { "9", CommonBoolCue.Digit9 },
		//        { "Misc_1", Misc_1Function },
		//        { "Misc_2", Misc_2Function },
		//    };
		//    LinkSmartObjectWithUserObjects(triList, smartObjectId, deviceUserObjects, sigDict, state);
		//}

		//public static void LinkDpadWithUserObjects(BasicTriListWithSmartObject triList,
		//    uint smartObjectId, List<CueActionPair> deviceUserObjects, bool state)
		//{
		//    var sigDict = new Dictionary<string, Cue>
		//    {
		//        { "Up", CommonBoolCue.Up },
		//        { "Down", CommonBoolCue.Down },
		//        { "Left", CommonBoolCue.Left },
		//        { "Right", CommonBoolCue.Right },
		//        { "OK", CommonBoolCue.Select },
		//    };
		//    LinkSmartObjectWithUserObjects(triList, smartObjectId, deviceUserObjects, sigDict, state);
		//}


		///// <summary>
		///// MOVE TO HELPER CLASS
		///// </summary>
		///// <param name="triList"></param>
		///// <param name="smartObjectId"></param>
		///// <param name="deviceUserObjects"></param>
		///// <param name="smartObjectSigMap"></param>
		///// <param name="state"></param>
		//public static void LinkSmartObjectWithUserObjects(BasicTriListWithSmartObject triList,
		//    uint smartObjectId, List<CueActionPair> deviceUserObjects, Dictionary<string, Cue> smartObjectSigMap, bool state)
		//{
		//    // Hook up smart objects if applicable
		//    if (triList.SmartObjects.Contains(smartObjectId))
		//    {
		//        var smartObject = triList.SmartObjects[smartObjectId];
		//        foreach (var kvp in smartObjectSigMap)
		//        {
		//            if (smartObject.BooleanOutput.Contains(kvp.Key))
		//            {
		//                if (state)
		//                {
		//                    // look for a user object and if so, attach/detach it to/from the sig.
		//                    var uo = deviceUserObjects.FirstOrDefault(ao => ao.Cue == kvp.Value);
		//                    if (uo != null)
		//                        smartObject.BooleanOutput[kvp.Key].UserObject = uo;
		//                }
		//                else
		//                    smartObject.BooleanOutput[kvp.Key].UserObject = null;
		//            }
		//            else
		//                Debug.Console(0, "Smart object {0} does not contain Sig {1}", smartObject.ID, kvp.Key);
		//        }
		//    }
		//    else
		//        Debug.Console(0, "ERROR Smart object {0} not found on {1:X2}", smartObjectId, triList.ID);
		//}
	}
}