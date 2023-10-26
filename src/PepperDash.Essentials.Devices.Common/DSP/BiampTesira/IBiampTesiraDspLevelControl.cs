using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.DSP
{
    public interface IBiampTesiraDspLevelControl : IBasicVolumeWithFeedback
    {
        /// <summary>
        /// In BiAmp: Instance Tag, QSC: Named Control, Polycom: 
        /// </summary>
        string ControlPointTag { get; }
        int Index1 { get; }
        int Index2 { get; }
        bool HasMute { get; }
        bool HasLevel { get; }
        bool AutomaticUnmuteOnVolumeUp { get; }
    }
}