using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Devices.Common.DSP
{

     //QUESTIONS:
	 
     //When subscribing, just use the Instance ID for Custom Name?
	
     //Verbose on subscriptions?

     //! "publishToken":"name" "value":-77.0
     //! "myLevelName" -77

    //public class TesiraForteMuteControl : IDspLevelControl
    //{
    //    BiampTesiraForteDsp Parent;
    //    bool _IsMuted;
    //    ushort _VolumeLevel;

    //    public TesiraForteMuteControl(string id, BiampTesiraForteDsp parent)
    //        : base(id)
    //    {
    //        Parent = parent;
    //    }

    //    public void Initialize()
    //    {

    //    }

    //    protected override Func<bool> MuteFeedbackFunc
    //    {
    //        get { return () => _IsMuted; }
    //    }

    //    protected override Func<int> VolumeLevelFeedbackFunc
    //    {
    //        get { return () => _VolumeLevel; }
    //    }

    //    public override void MuteOff()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void MuteOn()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void SetVolume(ushort level)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void MuteToggle()
    //    {
    //    }

    //    public override void VolumeDown(bool pressRelease)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void VolumeUp(bool pressRelease)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}