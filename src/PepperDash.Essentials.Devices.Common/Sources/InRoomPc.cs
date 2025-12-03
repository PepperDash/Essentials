using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Sources
{
    /// <summary>
    /// Represents a InRoomPc
    /// </summary>
    public class InRoomPc : EssentialsDevice, IHasFeedback, IRoutingSource, IRoutingOutputs, IAttachVideoStatus, IUiDisplayInfo, IUsageTracking
    {
        /// <summary>
        /// Gets or sets the DisplayUiType
        /// </summary>
        public uint DisplayUiType { get { return DisplayUiConstants.TypeLaptop; } }
        /// <summary>
        /// Gets or sets the IconName
        /// </summary>
        public string IconName { get; set; }
        /// <summary>
        /// Gets or sets the HasPowerOnFeedback
        /// </summary>
        public BoolFeedback HasPowerOnFeedback { get; private set; }

        /// <summary>
        /// Gets or sets the AnyVideoOut
        /// </summary>
        public RoutingOutputPort AnyVideoOut { get; private set; }

        #region IRoutingOutputs Members

        /// <summary>
        /// Gets or sets the OutputPorts
        /// </summary>
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="InRoomPc"/> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        public InRoomPc(string key, string name)
            : base(key, name)
        {
            IconName = "PC";
            HasPowerOnFeedback = new BoolFeedback("HasPowerFeedback",
                () => this.GetVideoStatuses() != VideoStatusOutputs.NoStatus);

            OutputPorts = new RoutingPortCollection<RoutingOutputPort>
            {
              (AnyVideoOut = new RoutingOutputPort(RoutingPortNames.AnyVideoOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.None, 0, this))
            };
        }

        #region IHasFeedback Members

        /// <summary>
        /// Passes through the VideoStatuses list
        /// </summary>
        public FeedbackCollection<Feedback> Feedbacks
        {
            get
            {
                var newList = new FeedbackCollection<Feedback>();
                newList.AddRange(this.GetVideoStatuses().ToList());
                return newList;
            }
        }

        #endregion

        #region IUsageTracking Members

        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion
    }

}