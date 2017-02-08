//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;

//namespace PepperDash.Essentials
//{
//    //***************************************************************************************************
//    public abstract class EssentialsRoom : Room
//    {
//        public event EventHandler<EssentialsRoomSourceChangeEventArgs> PresentationSourceChange;
//        public event EventHandler<EssentialsRoomAudioDeviceChangeEventArgs> AudioDeviceWillChange;
//        public Dictionary<uint, Device> Sources { get; protected set; }

//        public abstract BoolFeedback RoomIsOnStandby { get; protected set; }
//        public abstract BoolFeedback RoomIsOccupied { get; protected set; }

//        public uint UnattendedShutdownTimeMs { get; set; }

//        /// <summary>
//        /// For use when turning on room without a source selection - e.g. from
//        /// wake-on signal or occ sensor
//        /// </summary>
//        public SourceListItem DefaultPresentationSource { get; set; }
		
//#warning This might need more "guts" and shouldn't be public
//        public SourceListItem CurrentPresentationSourceInfo { get; set; }

//        //public IPresentationSource CurrentPresentationSource { get; protected set; }
//        //{
//        //    get
//        //    {
//        //        if (_CurrentPresentationSource == null)
//        //            _CurrentPresentationSource = PresentationDevice.Default;
//        //        return _CurrentPresentationSource;
//        //    }
//        //    protected set { _CurrentPresentationSource = value; }
//        //}
//        //IPresentationSource _CurrentPresentationSource;

//        /// <summary>
//        /// The volume control device for this room - changing it will trigger event
//        /// </summary>
//        public IBasicVolumeControls CurrentAudioDevice
//        {
//            get { return _CurrentAudioDevice; }
//            protected set
//            {
//                if (value != _CurrentAudioDevice)
//                    if (AudioDeviceWillChange != null)
//                        AudioDeviceWillChange(this, 
//                            new EssentialsRoomAudioDeviceChangeEventArgs(this, _CurrentAudioDevice, value));
//                _CurrentAudioDevice = value;
//            }
//        }
//        IBasicVolumeControls _CurrentAudioDevice;

//        public EssentialsRoom(string key, string name)
//            : base(key, name)
//        {
//        }

//        public virtual void SelectSource(uint sourceNum) { }

//        public virtual void SelectSource(IPresentationSource newSrc) { }

//        /// <summary>
//        /// Make sure that this is called before changing the source
//        /// </summary>
//        protected void OnPresentationSourceChange(SourceListItem currentSource, SourceListItem newSource)
//        {
//            var handler = PresentationSourceChange;
//            if (handler != null)
//                PresentationSourceChange(this,
//                    new EssentialsRoomSourceChangeEventArgs(this, currentSource, newSource));
//        }
//    }

//}