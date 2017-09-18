using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Requirements for a device that has dialing capabilities
    /// </summary>
    public interface IHasDialer
    {
        // Add requirements for Dialer functionality

        void Dial(string number);
        void EndCall();
        void AcceptCall();
        void RejectCall();

        void SendDtmf(string digit);

        IntFeedback ActiveCallCountFeedback { get; }
        BoolFeedback IncomingCallFeedback { get; }


        
    }

    /// <summary>
    /// Defines minimum volume controls for a codec device with dialing capabilities
    /// </summary>
    public interface ICodecAudio : IBasicVolumeWithFeedback, IPrivacy
    {

    }

    /// <summary>
    /// Adds control of codec receive volume
    /// </summary>
    public interface IReceiveVolume
    {
        // Break this out into 3 interfaces
        void SetReceiveVolume(ushort level);
        void ReceiveMuteOn();
        void ReceiveMuteOff();
        void ReceiveMuteToggle();
        IntFeedback ReceiveLevelFeedback { get; }
        BoolFeedback ReceiveMuteIsOnFeedback { get; }
    }

    /// <summary>
    /// Adds control of codec transmit volume
    /// </summary>
    public interface ITransmitVolume
    {
        void SetTransmitVolume(ushort level);
        void TransmitMuteOn();
        void TransmitMuteOff();
        void TransmitMuteToggle();
        IntFeedback TransmitLevelFeedback { get; }
        BoolFeedback TransmitMuteIsOnFeedback { get; }
    }

    /// <summary>
    /// Adds control of codec privacy function (microphone mute)
    /// </summary>
    public interface IPrivacy
    {
        void PrivacyModeOn();
        void PrivacyModeOff();
        void PrivacyModeToggle();
        BoolFeedback PrivacyModeIsOnFeedback { get; }
    }

    public interface IHasCallHistory
    {
        // Add recent calls list
    }

    public interface IHasDirectory
    {

    }

    public interface IHasObtp
    {

        // Upcoming Meeting warning event
    }
}