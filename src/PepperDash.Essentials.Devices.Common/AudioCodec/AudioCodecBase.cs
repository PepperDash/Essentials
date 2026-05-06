using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec;

/// <summary>
/// Base class for audio codecs. Provides common properties and methods for audio codecs, 
/// as well as a common implementation of IDialerCallStatus to allow the AudioCodecBaseMessenger 
/// to get call status information without requiring the full AudioCodecBase class. 
/// This is useful for devices that have dialer call status information but do not need to implement 
/// the full AudioCodecBase class.
/// </summary>
public abstract class AudioCodecBase : EssentialsDevice, IDialerCallStatus, IUsageTracking, IAudioCodecInfo
{
    /// <inheritdoc />
    public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

    /// <inheritdoc />
    public AudioCodecInfo CodecInfo { get; protected set; }

    #region IUsageTracking Members

    /// <summary>
    /// This object can be added by outside users of this class to provide usage tracking
    /// for various services
    /// </summary>
    public UsageTracking UsageTracker { get; set; }

    #endregion

    /// <summary>
    /// Returns true when any call is not in state Unknown, Disconnecting, Disconnected
    /// </summary>
    public bool IsInCall
    {
        get
        {
            bool value;

            if (ActiveCalls != null)
                value = ActiveCalls.Any(c => c.IsActiveCall);
            else
                value = false;
            return value;
        }
    }

    // In most cases only a single call can be active
    /// <inheritdoc />
    public List<CodecActiveCallItem> ActiveCalls { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    /// <param name="name"></param>
    public AudioCodecBase(string key, string name)
        : base(key, name)
    {
        ActiveCalls = new List<CodecActiveCallItem>();
    }

    /// <summary>
    /// Helper method to fire CallStatusChange event with old and new status
    /// </summary>
    protected void SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus newStatus, CodecActiveCallItem call)
    {
        call.Status = newStatus;

        OnCallStatusChange(call);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="previousStatus"></param>
    /// <param name="newStatus"></param>
    /// <param name="item"></param>
    protected void OnCallStatusChange(CodecActiveCallItem item)
    {
        var handler = CallStatusChange;
        if (handler != null)
            handler(this, new CodecCallStatusItemChangeEventArgs(item));

        if (UsageTracker != null)
        {
            if (IsInCall && !UsageTracker.UsageTrackingStarted)
                UsageTracker.StartDeviceUsage();
            else if (UsageTracker.UsageTrackingStarted && !IsInCall)
                UsageTracker.EndDeviceUsage();
        }
    }

    #region IHasDialer Members

    /// <inheritdoc />
    public abstract void Dial(string number);

    /// <inheritdoc />
    public abstract void EndCall(CodecActiveCallItem activeCall);

    /// <inheritdoc />
    public abstract void EndAllCalls();

    /// <inheritdoc />
    public abstract void AcceptCall(CodecActiveCallItem item);


    public abstract void RejectCall(CodecActiveCallItem item);


    public abstract void SendDtmf(string digit);

    #endregion
}