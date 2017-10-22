//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.SmartObjects;

//namespace PepperDash.Essentials
//{
//    public class SmartObjectHeaderButtonList : SmartObjectHelperBase
//    {

//        public SmartObjectHeaderButtonList(SmartObject so)
//            : base(so, true)
//        {

//        }
//    }

//    public class HeaderListButton
//    {
//        public BoolInputSig SelectedSig { get; private set; }
//        public BoolInputSig VisibleSig { get; private set; }
//        public BoolOutputSig OutputSig { get; private set; }
//        StringInputSig IconSig;
		
//        public HeaderListButton(SmartObjectHeaderButtonList list, uint index)
//        {
//            var so = list.SmartObject;
//            OutputSig = so.BooleanOutput["Item " + index + " Pressed"];
//            SelectedSig = so.BooleanInput["Item " + index + " Selected"];
//            VisibleSig = so.BooleanInput["Item " + index + " Visible"];
//            IconSig = so.StringInput["Set Item " + index + " Icon Serial"];
//        }

//        public void SetIcon(string i)
//        {
//            IconSig.StringValue = i;
//        }

//        public void ClearIcon()
//        {
//            IconSig.StringValue = "Blank";
//        }

//        public static string Calendar = "Calendar";
//        public static string Camera = "Camera";
//        public static string Gear = "Gear";
//        public static string Lights = "Lights";
//        public static string Help = "Help";
//        public static string OnHook = "DND";
//        public static string Phone = "Phone";
//    }
//}