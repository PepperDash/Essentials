using System;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines the eSourceListItemDestinationTypes enumeration, which represents the various destination types for source list items in a room control system. 
/// This enumeration is marked as obsolete, indicating that it may be removed in future versions and should not be used in new development. 
/// Each member of the enumeration corresponds to a specific type of display or audio output commonly found in room control systems, 
/// such as default displays, program audio, codec content, and auxiliary displays.
/// </summary>
[Obsolete]
public enum eSourceListItemDestinationTypes
{
    /// <summary>
    /// Default display, used for the main video output in a room
    /// </summary>
    defaultDisplay,
    /// <summary>
    /// Left display
    /// </summary>
    leftDisplay,
    /// <summary>
    /// Right display
    /// </summary>
    rightDisplay,
    /// <summary>
    /// Center display
    /// </summary>
    centerDisplay,
    /// <summary>
    /// Program audio, used for the main audio output in a room
    /// </summary>
    programAudio,
    /// <summary>
    /// Codec content, used for sharing content to the far end in a video call
    /// </summary>
    codecContent,
    /// <summary>
    /// Front left display, used for rooms with multiple displays
    /// </summary>
    frontLeftDisplay,
    /// <summary>
    /// Front right display, used for rooms with multiple displays
    /// </summary>
    frontRightDisplay,
    /// <summary>
    /// Rear left display, used for rooms with multiple displays
    /// </summary>
    rearLeftDisplay,
    /// <summary>
    /// Rear right display, used for rooms with multiple displays
    /// </summary>
    rearRightDisplay,
    /// <summary>
    /// Auxiliary display 1, used for additional displays in a room
    /// </summary>
    auxDisplay1,
    /// <summary>
    /// Auxiliary display 2, used for additional displays in a room
    /// </summary>
    auxDisplay2,
    /// <summary>
    /// Auxiliary display 3, used for additional displays in a room
    /// </summary>
    auxDisplay3,
    /// <summary>
    /// Auxiliary display 4, used for additional displays in a room
    /// </summary>
    auxDisplay4,
    /// <summary>
    /// Auxiliary display 5, used for additional displays in a room
    /// </summary>
    auxDisplay5,
    /// <summary>
    /// Auxiliary display 6, used for additional displays in a room
    /// </summary>
    auxDisplay6,
    /// <summary>
    /// Auxiliary display 7, used for additional displays in a room
    /// </summary>
    auxDisplay7,
    /// <summary>
    /// Auxiliary display 8, used for additional displays in a room
    /// </summary>
    auxDisplay8,
    /// <summary>
    /// Auxiliary display 9, used for additional displays in a room
    /// </summary>
    auxDisplay9,
    /// <summary>
    /// Auxiliary display 10, used for additional displays in a room
    /// </summary>
    auxDisplay10,
}