//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;

//using PepperDash.Core;


//namespace PepperDash.Essentials.Core
//{
//    public abstract class DvdDeviceBase : Device, IPresentationSource, IHasCueActionList 
//    {
//        public DvdDeviceBase(string key, string name)
//            : base(key, name)
//        {
//            HasPowerOnFeedback = new BoolFeedback(() => false);

//        }

//        #region IPresentationSource Members

//        PresentationSourceType IPresentationSource.Type
//        {
//            get { return PresentationSourceType.Dvd; }
//        }

//        public string IconName
//        {
//            get
//            {
//                return "DVD";
//            }
//            set { }
//        }

//        public virtual BoolFeedback HasPowerOnFeedback { get; private set; }

//        #endregion

//        #region IFunctionList Members

//        public abstract List<CueActionPair> CueActionList { get; }

//        #endregion

//    }
//}