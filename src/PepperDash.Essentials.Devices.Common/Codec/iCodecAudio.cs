﻿using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines minimum volume controls for a codec device with dialing capabilities
    /// </summary>
    public interface ICodecAudio : IBasicVolumeWithFeedback, IPrivacy
    {

    }
}