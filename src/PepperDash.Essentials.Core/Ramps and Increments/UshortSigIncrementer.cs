using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Attaches to UShortInputSig and does incremental ramping of the signal 
/// </summary>
public class UshortSigIncrementer
{
    UShortInputSig TheSig;

    /// <summary>
    /// Amount to change the signal on each step
    /// </summary>
    public ushort ChangeAmount { get; set; }
    /// <summary>
    /// Maximum value to ramp to
    /// </summary>
    public int MaxValue { get; set; }
    /// <summary>
    /// Minimum value to ramp to
    /// </summary>
    public int MinValue { get; set; }
    /// <summary>
    /// The delay before the incrementer starts repeating
    /// </summary>
    public uint RepeatDelay { get; set; }
    /// <summary>
    /// The time interval between each repeat
    /// </summary>
    public uint RepeatTime { get; set; }
    bool SignedMode;
    Timer Timer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sig"></param>
    /// <param name="changeAmount"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="repeatDelay"></param>
    /// <param name="repeatTime"></param>
    public UshortSigIncrementer(UShortInputSig sig, ushort changeAmount, int minValue, int maxValue, uint repeatDelay, uint repeatTime)
    {
        TheSig = sig;
        ChangeAmount = changeAmount;
        MaxValue = maxValue;
        MinValue = minValue;
        if (MinValue < 0 || MaxValue < 0) SignedMode = true;
        RepeatDelay = repeatDelay;
        RepeatTime = repeatTime;
        if (SignedMode && (MinValue < -32768 || MaxValue > 32767))
            Debug.LogMessage(LogEventLevel.Debug, "UshortSigIncrementer has signed values that exceed range of -32768, 32767");
    }

    /// <summary>
    /// Starts incrementing cycle
    /// </summary>

    public void StartUp()
    {
        if (Timer != null) return;
        Go(ChangeAmount);
    }

    /// <summary>
    /// Starts decrementing cycle
    /// </summary>
    public void StartDown()
    {
        if (Timer != null) return;
        Go(-ChangeAmount);
    }

    void Go(int change)
    {
        int level;
        if (SignedMode) level = TheSig.ShortValue;
        else level = TheSig.UShortValue;

        // Fire once then pause
        int newLevel = level + change;
        bool atLimit = CheckLevel(newLevel, out newLevel);
        SetValue((ushort)newLevel);


        if (atLimit) // Don't go past end
            Stop();
        else if (Timer == null) // Only enter the timer if it's not already running
        {
            Timer = new Timer(RepeatDelay) { AutoReset = false };
            Timer.Elapsed += (s, e) =>
            {
                Go(change);
                if (Timer != null)
                {
                    Timer.Interval = RepeatTime;
                    Timer.AutoReset = true;
                    Timer.Start();
                }
            };
            Timer.Start();
        }
    }

    bool CheckLevel(int levelIn, out int levelOut)
    {
        bool IsAtLimit = false;
        if (levelIn > MaxValue)
        {
            levelOut = MaxValue;
            IsAtLimit = true;
        }
        else if (levelIn < MinValue)
        {
            levelOut = MinValue;
            IsAtLimit = true;
        }
        else
            levelOut = levelIn;
        return IsAtLimit;
    }

    /// <summary>
    /// Stops incrementing/decrementing cycle
    /// </summary>
    public void Stop()
    {
        if (Timer != null)
            Timer.Stop();
        Timer = null;
    }

    /// <summary>
    /// Sets the value of the signal
    /// </summary>
    /// <param name="value"></param>

    void SetValue(ushort value)
    {
        //CrestronConsole.PrintLine("Increment level:{0} / {1}", value, (short)value);
        TheSig.UShortValue = value;
    }
}