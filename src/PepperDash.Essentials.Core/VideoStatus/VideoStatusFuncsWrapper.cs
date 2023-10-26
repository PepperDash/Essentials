using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Use this class to pass in values to RoutingInputPorts.  Unused properties will have default 
    /// funcs assigned to them.
    /// </summary>
    public class VideoStatusFuncsWrapper
    {
        public Func<bool> HasVideoStatusFunc { get; set; }
        public Func<bool> HdcpActiveFeedbackFunc { get; set; }
        public Func<string> HdcpStateFeedbackFunc { get; set; }
        public Func<string> VideoResolutionFeedbackFunc { get; set; }
        public Func<bool> VideoSyncFeedbackFunc { get; set; }

        public VideoStatusFuncsWrapper()
        {
            HasVideoStatusFunc          = () => true;
            HdcpActiveFeedbackFunc      = () => false;
            HdcpStateFeedbackFunc       = () => "";
            VideoResolutionFeedbackFunc = () => "n/a";
            VideoSyncFeedbackFunc       = () => false;
        }
    }
}