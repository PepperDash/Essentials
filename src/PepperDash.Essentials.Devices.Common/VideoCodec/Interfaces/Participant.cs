namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Represents a call participant
    /// </summary>
    public class Participant
    {
        public int UserId { get; set; }
        public bool IsHost { get; set; }
        public bool IsMyself { get; set; }
        public string Name { get; set; }
        public bool CanMuteVideo { get; set; }
        public bool CanUnmuteVideo { get; set; }
        public bool VideoMuteFb { get; set; }
        public bool AudioMuteFb { get; set; }
        public bool HandIsRaisedFb { get; set; }
        public bool IsPinnedFb { get; set; }
        public int ScreenIndexIsPinnedToFb { get; set; }

        public Participant()
        {
            // Initialize to -1 (no screen)
            ScreenIndexIsPinnedToFb = -1;
        }
    }
}