using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IHasBatteryStats
    /// </summary>
    public interface IHasBatteryStats : IKeyName
    {
        /// <summary>
        /// Gets the BatteryPercentage
        /// </summary>
        int BatteryPercentage { get; }

        /// <summary>
        /// Gets the BatteryCautionThresholdPercentage
        /// </summary>
        int BatteryCautionThresholdPercentage { get;  }

        /// <summary>
        /// Gets the BatteryWarningThresholdPercentage
        /// </summary>
        int BatteryWarningThresholdPercentage { get; }

        /// <summary>
        /// Gets the BatteryIsWarningFeedback
        /// </summary>
        BoolFeedback BatteryIsWarningFeedback { get; }

        /// <summary>
        /// Gets the BatteryIsCautionFeedback
        /// </summary>
        BoolFeedback BatteryIsCautionFeedback { get; }

        /// <summary>
        /// Gets the BatteryIsOkFeedback
        /// </summary>
        BoolFeedback BatteryIsOkFeedback { get; }

        /// <summary>
        /// Gets the BatteryPercentageFeedback
        /// </summary>
        IntFeedback BatteryPercentageFeedback { get; }
    }

    /// <summary>
    /// Defines the contract for IHasBatteryCharging
    /// </summary>
    public interface IHasBatteryCharging : IHasBatteryStats
    {
        /// <summary>
        /// Gets the BatteryIsCharging
        /// </summary>
        BoolFeedback BatteryIsCharging { get; }
    }

    /// <summary>
    /// Interface for any device that has multiple batteries that can be monitored
    /// </summary>
    public interface IHasBatteries : IKeyName
    {
        /// <summary>
        /// Collection of batteries
        /// </summary>
        ReadOnlyDictionary<string, IHasBatteryStats> Batteries { get; }   
    }

    /// <summary>
    /// Defines the contract for IHasBatteryStatsExtended
    /// </summary>
    public interface IHasBatteryStatsExtended : IHasBatteryStats
    {
        /// <summary>
        /// Gets the InputVoltage in millivolts
        /// </summary>
        int InputVoltage { get; }

        /// <summary>
        /// Gets the OutputVoltage in millivolts
        /// </summary>
        int OutputVoltage { get; }

        /// <summary>
        /// Gets the InputCurrent in milliamps
        /// </summary>
        int InptuCurrent { get; }

        /// <summary>
        /// Gets the OutputCurrent in milliamps
        /// </summary>
        int OutputCurrent { get; }

        /// <summary>
        /// Gets the InputVoltageFeedback
        /// </summary>
        IntFeedback InputVoltageFeedback { get; }

        /// <summary>
        /// Gets the OutputVoltageFeedback
        /// </summary>
        IntFeedback OutputVoltageFeedback { get; }

        /// <summary>
        /// Gets the InputCurrentFeedback
        /// </summary>
        IntFeedback InputCurrentFeedback { get; }

        /// <summary>
        /// Gets the OutputCurrentFeedback
        /// </summary>
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