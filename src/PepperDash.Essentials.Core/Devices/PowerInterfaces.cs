using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Interface for any device that has a battery that can be monitored
    /// </summary>
    public interface IHasBatteryStats : IKeyName
    {
        int BatteryPercentage { get; }
        int BatteryCautionThresholdPercentage { get;  }
        int BatteryWarningThresholdPercentage { get; }
        BoolFeedback BatteryIsWarningFeedback { get; }
        BoolFeedback BatteryIsCautionFeedback { get; }
        BoolFeedback BatteryIsOkFeedback { get; }
        IntFeedback BatteryPercentageFeedback { get; }
    }

    /// <summary>
    /// Interface for any device that has a battery that can be monitored and the ability to charge and discharge
    /// </summary>
    public interface IHasBatteryCharging : IHasBatteryStats
    {
        BoolFeedback BatteryIsCharging { get; }
    }

    /// <summary>
    /// Interface for any device that has multiple batteries that can be monitored
    /// </summary>
    public interface IHasBatteries : IKeyName
    {
        ReadOnlyDictionary<string, IHasBatteryStats> Batteries { get; }   
    }

    public interface IHasBatteryStatsExtended : IHasBatteryStats
    {
        int InputVoltage { get; }
        int OutputVoltage { get; }
        int InptuCurrent { get; }
        int OutputCurrent { get; }

        IntFeedback InputVoltageFeedback { get; }
        IntFeedback OutputVoltageFeedback { get; }
        IntFeedback InputCurrentFeedback { get; }
        IntFeedback OutputCurrentFeedback { get; }
    }
}