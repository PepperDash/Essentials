using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core;

public class FeedbackEventArgs : EventArgs
{
    public bool BoolValue { get; private set; }
    public int IntValue { get; private set; }
    public ushort UShortValue
    {
        get
        {
            return (ushort)IntValue;
        }
    }
    public string StringValue { get; private set; }
    public eFeedbackEventType Type { get; private set; }

    public FeedbackEventArgs(bool value)
    {
        BoolValue = value;
        Type = eFeedbackEventType.TypeBool;
    }

    public FeedbackEventArgs(int value)
    {
        IntValue = value;
        Type = eFeedbackEventType.TypeInt;
    }

    public FeedbackEventArgs(string value)
    {
        StringValue = value;
        Type = eFeedbackEventType.TypeString;
    }
}

public enum eFeedbackEventType
{
    TypeBool,
    TypeInt,
    TypeString
}