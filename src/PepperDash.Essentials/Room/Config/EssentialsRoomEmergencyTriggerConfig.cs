namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsRoomEmergencyTriggerConfig
    {
        /// <summary>
        /// contact,
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Input number if contact
        /// </summary>
        public int Number { get; set; }

        public bool TriggerOnClose { get; set; }

    }
}