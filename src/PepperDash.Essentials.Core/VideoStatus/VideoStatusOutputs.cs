using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.VideoStatus
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
			HasVideoStatusFunc = () => true;
			HdcpActiveFeedbackFunc = () => false;
			HdcpStateFeedbackFunc = () => "";
			VideoResolutionFeedbackFunc = () => "n/a";
			VideoSyncFeedbackFunc = () => false;
		}
	}

	/// <summary>
	/// Wraps up the common video statuses exposed on a video input port
	/// </summary>
	public class VideoStatusOutputs
	{
		public BoolFeedback HasVideoStatusFeedback { get; private set; }
		public BoolFeedback HdcpActiveFeedback { get; private set; }
		public StringFeedback HdcpStateFeedback { get; private set; }
		public StringFeedback VideoResolutionFeedback { get; private set; }
		public BoolFeedback VideoSyncFeedback { get; private set; }

		/// <summary>
		/// Gets the default, empty status group.
		/// </summary>
		public static VideoStatusOutputs NoStatus { get { return _Default; } }
		static VideoStatusOutputs _Default = new VideoStatusOutputs(new VideoStatusFuncsWrapper
			{
				HasVideoStatusFunc = () => false
			});

		public VideoStatusOutputs(VideoStatusFuncsWrapper funcs)
		{
			HasVideoStatusFeedback = new BoolFeedback("HasVideoStatusFeedback", funcs.HasVideoStatusFunc);
			HdcpActiveFeedback = new BoolFeedback("HdcpActiveFeedback", funcs.HdcpActiveFeedbackFunc);
			HdcpStateFeedback = new StringFeedback("HdcpStateFeedback", funcs.HdcpStateFeedbackFunc);
			VideoResolutionFeedback = new StringFeedback("VideoResolutionFeedback", 
				funcs.VideoResolutionFeedbackFunc);
			VideoSyncFeedback = new BoolFeedback("VideoSyncFeedback", funcs.VideoSyncFeedbackFunc);
		}

		public void FireAll()
		{
			HasVideoStatusFeedback.FireUpdate();
			HdcpActiveFeedback.FireUpdate();
			HdcpActiveFeedback.FireUpdate();
			VideoResolutionFeedback.FireUpdate();
			VideoSyncFeedback.FireUpdate();
		}

		public List<Feedback> ToList()
		{
			return new List<Feedback>
			{
				HasVideoStatusFeedback,
				HdcpActiveFeedback,
				HdcpStateFeedback,
				VideoResolutionFeedback,
				VideoSyncFeedback
			};
		}
	}

	///// <summary>
	///// Wraps up the common video statuses exposed on a video input port
	///// </summary>
	//public class BasicVideoStatus : IBasicVideoStatus
	//{
	//    public event VideoStatusChangeHandler VideoStatusChange;

	//    public bool HasVideoStatus { get; private set; }

	//    public bool HdcpActive
	//    {
	//        get { return HdcpActiveFunc != null ? HdcpActiveFunc() : false; }
	//    }

	//    public string HdcpState
	//    {
	//        get { return HdcpStateFunc != null? HdcpStateFunc() : ""; }
	//    }

	//    public string VideoResolution
	//    {
	//        get { return VideoResolutionFunc != null ? VideoResolutionFunc() : ""; }
	//    }

	//    public bool VideoSync
	//    {
	//        get { return VideoSyncFunc != null ? VideoSyncFunc() : false; }
	//    }

	//    Func<bool> HdcpActiveFunc;
	//    Func<string> HdcpStateFunc;
	//    Func<string> VideoResolutionFunc;
	//    Func<bool> VideoSyncFunc;

	//    public BasicVideoStatus(bool hasVideoStatus, Func<bool> hdcpActiveFunc,
	//        Func<string> hdcpStateFunc, Func<string> videoResolutionFunc, Func<bool> videoSyncFunc)
	//    {
	//        HasVideoStatus = hasVideoStatus;
	//        HdcpActiveFunc = hdcpActiveFunc;
	//        HdcpStateFunc = hdcpStateFunc;
	//        VideoResolutionFunc = videoResolutionFunc;
	//        VideoSyncFunc = videoSyncFunc;
	//    }
	//}

	//public enum eVideoStatusChangeType
	//{
	//    HdcpActive,
	//    HdcpState,
	//    VideoResolution,
	//    VideoSync
	//}

	//public interface IBasicVideoStatus
	//{
	//    event VideoStatusChangeHandler VideoStatusChange;
	//    bool HdcpActive { get; }
	//    string HdcpState { get; }
	//    string VideoResolution { get; }
	//    bool VideoSync { get; }
	//}

	//public delegate void VideoStatusChangeHandler(IBasicVideoStatus device, eVideoStatusChangeType type);
}