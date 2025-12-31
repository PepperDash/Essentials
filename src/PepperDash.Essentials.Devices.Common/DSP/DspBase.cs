using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Devices.Common.DSP
{
    /// <summary>
    /// Base class for DSP devices
    /// </summary>
    public abstract class DspBase : EssentialsDevice, ILevelControls
    {
        /// <summary>
        /// Gets the collection of level control points
        /// </summary>
        public Dictionary<string, IBasicVolumeWithFeedback> LevelControlPoints { get; private set; }

        /// <summary>
        /// Gets the collection of dialer control points
        /// </summary>
        public Dictionary<string, DspControlPoint> DialerControlPoints { get; private set; }

        /// <summary>
        /// Gets the collection of switcher control points
        /// </summary>
        public Dictionary<string, DspControlPoint> SwitcherControlPoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DspBase class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        public DspBase(string key, string name) :
                base(key, name)
        {

            LevelControlPoints = new Dictionary<string, IBasicVolumeWithFeedback>();
            DialerControlPoints = new Dictionary<string, DspControlPoint>();
            SwitcherControlPoints = new Dictionary<string, DspControlPoint>();
        }


        // in audio call feedback

        // VOIP
        // Phone dialer

    }

    // Fusion
    // Privacy state
    // Online state
    // level/mutes ?

    // AC Log call stats

    // Typical presets:
    // call default preset to restore levels and mutes

    /// <summary>
    /// Base class for DSP control points
    /// </summary>
    public abstract class DspControlPoint : IKeyName
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DspControlPoint class
        /// </summary>
        /// <param name="key">The control point key</param>
        protected DspControlPoint(string key) => Key = key;
    }

    /// <summary>
    /// Base class for DSP level control points with volume and mute functionality
    /// </summary>
    public abstract class DspLevelControlPoint : DspControlPoint, IBasicVolumeWithFeedback
    {
        /// <summary>
        /// Gets or sets the MuteFeedback
        /// </summary>
        public BoolFeedback MuteFeedback { get; }
        /// <summary>
        /// Gets or sets the VolumeLevelFeedback
        /// </summary>
        public IntFeedback VolumeLevelFeedback { get; }

        /// <summary>
        /// Initializes a new instance of the DspLevelControlPoint class
        /// </summary>
        /// <param name="key">The control point key</param>
        /// <param name="muteFeedbackFunc">Function to get mute status</param>
        /// <param name="volumeLevelFeedbackFunc">Function to get volume level</param>
        protected DspLevelControlPoint(string key, Func<bool> muteFeedbackFunc, Func<int> volumeLevelFeedbackFunc) : base(key)
        {
            MuteFeedback = new BoolFeedback("mute", muteFeedbackFunc);
            VolumeLevelFeedback = new IntFeedback("volume", volumeLevelFeedbackFunc);
        }

        /// <summary>
        /// Turns mute off
        /// </summary>
        public abstract void MuteOff();
        /// <summary>
        /// Turns mute on
        /// </summary>
        public abstract void MuteOn();
        /// <summary>
        /// Toggles mute state
        /// </summary>
        public abstract void MuteToggle();
        /// <summary>
        /// Sets the volume level
        /// </summary>
        /// <param name="level">The volume level to set</param>
        public abstract void SetVolume(ushort level);
        /// <summary>
        /// Decreases volume
        /// </summary>
        /// <param name="pressRelease">True when pressed, false when released</param>
        public abstract void VolumeDown(bool pressRelease);
        /// <summary>
        /// Increases volume
        /// </summary>
        /// <param name="pressRelease">True when pressed, false when released</param>
        public abstract void VolumeUp(bool pressRelease);
    }

    /// <summary>
    /// Base class for DSP dialer control points
    /// </summary>
    public abstract class DspDialerBase : DspControlPoint
    {
        /// <summary>
        /// Initializes a new instance of the DspDialerBase class
        /// </summary>
        /// <param name="key">The dialer control point key</param>
        protected DspDialerBase(string key) : base(key) { }
    }


    // Main program 
    // VTC 
    // ATC
    // Mics, unusual

}