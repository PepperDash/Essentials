//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;

//using PepperDash.Essentials.Core.Presets;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;

//namespace PepperDash.Essentials
//{

//    public class PageControllerLargeSetTopBoxGeneric : DevicePageControllerBase
//    {
//        // To-DO: Add properties for component subpage names. DpadPos1, DpadPos2...
//        // Derived classes can then insert special subpages for variations on given
//        // device types.  Like DirecTV vs Comcast

//        public uint DpadSmartObjectId { get; set; }
//        public uint NumberPadSmartObjectId { get; set; }
//        public uint PresetsSmartObjectId { get; set; }
//        public uint Position5TabsId { get; set; }

//        ISetTopBoxControls Device;
//        DevicePresetsView PresetsView;


//        bool ShowPosition5Tabs;
//        uint CurrentVisiblePosition5Item = 1;
//        Dictionary<uint, uint> Position5SubpageJoins = new Dictionary<uint, uint>
//        {
//            { 1, 10053 },
//            { 2, 10054 }
//        };

//        public PageControllerLargeSetTopBoxGeneric(BasicTriListWithSmartObject tl, ISetTopBoxControls device)
//            : base(tl)
//        {
//            Device = device;
//            DpadSmartObjectId = 10011;
//            NumberPadSmartObjectId = 10014;
//            PresetsSmartObjectId = 10012;
//            Position5TabsId = 10081;

//            bool dpad = device is IDPad;
//            bool preset = device.HasPresets;
//            bool dvr = device.HasDvr;
//            bool numbers = device is INumericKeypad;
//            uint[] joins = null;

//            if (dpad && !preset && !dvr && !numbers) joins = new uint[] { 10031, 10091 };
//            else if (!dpad && preset && !dvr && !numbers) joins = new uint[] { 10032, 10091 };
//            else if (!dpad && !preset && dvr && !numbers) joins = new uint[] { 10033, 10091 };
//            else if (!dpad && !preset && !dvr && numbers) joins = new uint[] { 10034, 10091 };

//            else if (dpad && preset && !dvr && !numbers) joins = new uint[] { 10042, 10021, 10092 };
//            else if (dpad && !preset && dvr && !numbers) joins = new uint[] { 10043, 10021, 10092 };
//            else if (dpad && !preset && !dvr && numbers) joins = new uint[] { 10044, 10021, 10092 };
//            else if (!dpad && preset && dvr && !numbers) joins = new uint[] { 10043, 10022, 10092 };
//            else if (!dpad && preset && !dvr && numbers) joins = new uint[] { 10044, 10022, 10092 };
//            else if (!dpad && !preset && dvr && numbers) joins = new uint[] { 10044, 10023, 10092 };

//            else if (dpad && preset && dvr && !numbers) joins = new uint[] { 10053, 10032, 10011, 10093 };
//            else if (dpad && preset && !dvr && numbers) joins = new uint[] { 10054, 10032, 10011, 10093 };
//            else if (dpad && !preset && dvr && numbers) joins = new uint[] { 10054, 10033, 10011, 10093 };
//            else if (!dpad && preset && dvr && numbers) joins = new uint[] { 10054, 10033, 10012, 10093 };

//            else if (dpad && preset && dvr && numbers)
//            {
//                joins = new uint[] { 10081, 10032, 10011, 10093 }; // special case
//                ShowPosition5Tabs = true;
//            }
//            // Project the joins into corresponding sigs.
//            FixedObjectSigs = joins.Select(u => TriList.BooleanInput[u]).ToList();

//            // Build presets
//            if (device.HasPresets)
//            {
//                PresetsView = new DevicePresetsView(tl, device.PresetsModel);
//            }
//        }

//        protected override void CustomSetVisible(bool state)
//        {
//            if (ShowPosition5Tabs)
//            {
//                // Show selected tab
//                TriList.BooleanInput[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = state;

//                var tabSo = TriList.SmartObjects[Position5TabsId];
//                if (state) // Link up the tab object
//                {
//                    tabSo.BooleanOutput["Tab Button 1 Press"].UserObject = new Action<bool>(b => ShowTab(1));
//                    tabSo.BooleanOutput["Tab Button 2 Press"].UserObject = new Action<bool>(b => ShowTab(2));
//                }
//                else // Disco tab object
//                {
//                    tabSo.BooleanOutput["Tab Button 1 Press"].UserObject = null;
//                    tabSo.BooleanOutput["Tab Button 2 Press"].UserObject = null;
//                }
//            }

//            // Hook up smart objects if applicable
//#warning hook these up
//            //if (Device is IHasCueActionList)
//            //{
//            //    var uos = (Device as IHasCueActionList).CueActionList;
//            //    SmartObjectHelper.LinkDpadWithUserObjects(TriList, DpadSmartObjectId, uos, state);
//            //    SmartObjectHelper.LinkNumpadWithUserObjects(TriList, NumberPadSmartObjectId,
//            //        uos, CommonBoolCue.Dash, CommonBoolCue.Last, state);
//            //}


//            // Link, unlink presets
//            if (Device.HasPresets && state)
//                PresetsView.Attach();
//            else if (Device.HasPresets && !state)
//                PresetsView.Detach();
//        }

//        void ShowTab(uint number)
//        {
//            // Ignore re-presses
//            if (CurrentVisiblePosition5Item == number) return;
//            // Swap subpage
//            var bi = TriList.BooleanInput;
//            if (CurrentVisiblePosition5Item > 0)
//                bi[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = false;
//            CurrentVisiblePosition5Item = number;
//            bi[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = true;

//            // Show feedback on buttons
//        }
//    }
//}