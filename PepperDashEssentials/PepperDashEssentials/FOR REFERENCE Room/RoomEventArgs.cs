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
//    public class EssentialsRoomSourceChangeEventArgs : EventArgs
//    {
//        public EssentialsRoom Room { get; private set; }
//        public SourceListItem OldSource { get; private set; }
//        public SourceListItem NewSource { get; private set; }

//        public EssentialsRoomSourceChangeEventArgs(EssentialsRoom room,
//            SourceListItem oldSource, SourceListItem newSource)
//        {
//            Room = room;
//            OldSource = oldSource;
//            NewSource = newSource;
//        }
//    }



//    public class EssentialsRoomAudioDeviceChangeEventArgs : EventArgs
//    {
//        public EssentialsRoom Room { get; private set; }
//        public IBasicVolumeControls OldDevice { get; private set; }
//        public IBasicVolumeControls NewDevice { get; private set; }

//        public EssentialsRoomAudioDeviceChangeEventArgs(EssentialsRoom room,
//            IBasicVolumeControls oldDevice, IBasicVolumeControls newDevice)
//        {
//            Room = room;
//            OldDevice = oldDevice;
//            NewDevice = newDevice;
//        }
//    }

//}