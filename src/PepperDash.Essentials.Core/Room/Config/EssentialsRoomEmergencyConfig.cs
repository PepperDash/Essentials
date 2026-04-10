namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Represents a EssentialsRoomEmergencyConfig
    /// </summary>
    public class EssentialsRoomEmergencyConfig
    {
        /// <summary>
        /// Gets or sets the Trigger
        /// </summary>
        public EssentialsRoomEmergencyTriggerConfig Trigger { get; set; }

        /// <summary>
        /// Gets or sets the Behavior
        /// </summary>
        public string Behavior { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsRoomEmergencyTriggerConfig
    /// </summary>
    public class EssentialsRoomEmergencyTriggerConfig
    {
        /// <summary>
        /// contact,versiport
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Input number if contact
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// TriggerOnClose indicates if the trigger is on close
        /// </summary>
        public bool TriggerOnClose { get; set; }

    }
}