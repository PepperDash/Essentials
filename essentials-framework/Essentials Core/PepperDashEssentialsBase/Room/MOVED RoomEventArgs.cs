//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;

//namespace PepperDash.Essentials.Core
//{
//    public class EssentialsRoomSourceChangeEventArgs : EventArgs
//    {
//        public EssentialsRoom Room { get; private set; }
//        public IPresentationSource OldSource { get; private set; }
//        public IPresentationSource NewSource { get; private set; }

//        public EssentialsRoomSourceChangeEventArgs(EssentialsRoom room, 
//            IPresentationSource oldSource, IPresentationSource newSource)
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