namespace PepperDash.Essentials.Devices.Common.Codec
{

    /// <summary>
    /// Defines the contract for IHasScheduleAwareness
    /// </summary>
    public interface IHasScheduleAwareness
    {
        /// <summary>
        /// Gets the CodecScheduleAwareness instance
        /// </summary>
        CodecScheduleAwareness CodecSchedule { get; }

        /// <summary>
        /// Method to initiate getting the schedule from the server
        /// </summary>
        void GetSchedule();
    }

}
