using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Adds control of codec receive volume
    /// </summary>
    public interface IReceiveVolume
    {
        // Break this out into 3 interfaces

        /// <summary>
        /// Sets the receive volume level
        /// </summary>
        /// <param name="level">volume level to set</param>
        void SetReceiveVolume(ushort level);

        /// <summary>
        /// Mutes the receive audio
        /// </summary>
        void ReceiveMuteOn();

        /// <summary>
        /// Unmutes the receive audio
        /// </summary>
        void ReceiveMuteOff();

        /// <summary>
        /// Toggles the receive mute state
        /// </summary>
        void ReceiveMuteToggle();

        /// <summary>
        /// Feedback for the receive volume level
        /// </summary>
        IntFeedback ReceiveLevelFeedback { get; }

        /// <summary>
        /// Feedback for the receive mute state
        /// </summary>
        BoolFeedback ReceiveMuteIsOnFeedback { get; }
    }

    /// <summary>
    /// Defines the contract for ITransmitVolume
    /// </summary>
    public interface ITransmitVolume
    {
        /// <summary>
        /// Sets the transmit volume level
        /// </summary>
        /// <param name="level">volume level to set</param>
        void SetTransmitVolume(ushort level);

        /// <summary>
        /// Mutes the transmit audio
        /// </summary>
        void TransmitMuteOn();

        /// <summary>
        /// Unmutes the transmit audio
        /// </summary>
        void TransmitMuteOff();

        /// <summary>
        /// Toggles the transmit mute state
        /// </summary>
        void TransmitMuteToggle();

        /// <summary>
        /// Feedback for the transmit volume level
        /// </summary>
        IntFeedback TransmitLevelFeedback { get; }

        /// <summary>
        /// Feedback for the transmit mute state
        /// </summary>
        BoolFeedback TransmitMuteIsOnFeedback { get; }
    }

    /// <summary>
    /// Defines the contract for IPrivacy
    /// </summary>
    public interface IPrivacy
    {
        /// <summary>
        /// Enables privacy mode
        /// </summary>
        void PrivacyModeOn();

        /// <summary>
        /// Disables privacy mode
        /// </summary>
        void PrivacyModeOff();

        /// <summary>
        /// Toggles privacy mode
        /// </summary>
        void PrivacyModeToggle();

        /// <summary>
        /// Feedback for the privacy mode state
        /// </summary>
        BoolFeedback PrivacyModeIsOnFeedback { get; }
    }
}