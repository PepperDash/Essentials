using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a FeedbackEventArgs
    /// </summary>
    public class FeedbackEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the BoolValue
        /// </summary>
        public bool BoolValue { get; private set; }

        /// <summary>
        /// Gets or sets the IntValue
        /// </summary>
        public int IntValue { get; private set; }

        /// <summary>
        /// Gets or sets the UShortValue
        /// </summary>
        public ushort UShortValue
        {
            get
            {
                return (ushort)IntValue;
            }
        }

        /// <summary>
        /// Gets or sets the StringValue
        /// </summary>
        public string StringValue { get; private set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public eFeedbackEventType Type { get; private set; }

        /// <summary>
        /// Constructor for BoolValue
        /// </summary>
        /// <param name="value">value to set</param>
        public FeedbackEventArgs(bool value)
        {
            BoolValue = value;
            Type = eFeedbackEventType.TypeBool;
        }

        /// <summary>
        /// Constructor for IntValue
        /// </summary>
        /// <param name="value">value to set</param>
        public FeedbackEventArgs(int value)
        {
            IntValue = value;
            Type = eFeedbackEventType.TypeInt;
        }

        /// <summary>
        /// Constructor for StringValue
        /// </summary>
        /// <param name="value">value to set</param>
        public FeedbackEventArgs(string value)
        {
            StringValue = value;
            Type = eFeedbackEventType.TypeString;
        }
    }

    /// <summary>
    /// Enumeration of eFeedbackEventType values
    /// </summary>
    public enum eFeedbackEventType
    {
        /// <summary>
        /// Boolean type feedback event
        /// </summary>
        TypeBool,

        /// <summary>
        /// Integer type feedback event
        /// </summary>
        TypeInt,

        /// <summary>
        /// String type feedback event
        /// </summary>
        TypeString
    }
}