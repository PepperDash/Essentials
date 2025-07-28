using Crestron.SimplSharp;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Defines the contract for IHasBatteryStats
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
    /// Defines the contract for IHasBatteryCharging
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
    /// Defines the contract for IHasPowerCycleWithBattery
    /// </summary>
    public interface IHasPowerCycleWithBattery : IHasPowerCycle, IHasBatteryStats
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