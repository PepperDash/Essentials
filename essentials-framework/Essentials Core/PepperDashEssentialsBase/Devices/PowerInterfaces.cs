using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
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

    /// <summary>
    /// Interface for any device that is able to control its power, has a configurable reboot time, and has batteries that can be monitored
    /// </summary>
    public interface IHasPowerCycleWithBatteries : IHasPowerCycle, IHasBatteryStats
    {
        
    }

    /// <summary>
    /// Interface for any device that is able to control it's power and has a configurable reboot time
    /// </summary>
    public interface IHasPowerCycle : IKeyName, IHasPowerControlWithFeedback
    {
        /// <summary>
        /// Delay between power off and power on for reboot
        /// </summary>
        int PowerCycleTimeMs { get; }

        /// <summary>
        /// Reboot outlet
        /// </summary>
        void PowerCycle();
    }

    /// <summary>
    /// Interface for any device that contains a collection of IHasPowerReboot Devices
    /// </summary>
    public interface IHasControlledPowerOutlets : IKeyName
    {
        /// <summary>
        /// Collection of IPduOutlets
        /// </summary>
        ReadOnlyDictionary<int, IHasPowerCycle> PduOutlets { get; }

    }



}