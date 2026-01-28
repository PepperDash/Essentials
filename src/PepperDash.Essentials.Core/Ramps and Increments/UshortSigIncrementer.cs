using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Attaches to UShortInputSig and does incremental ramping of the signal 
    /// </summary>
    public class UshortSigIncrementer
    {
        UShortInputSig TheSig;

        /// <summary>
        /// The amount to change the value by each increment
        /// </summary>
        public ushort ChangeAmount { get; set; }

        /// <summary>
        /// The maximum value for the incrementer
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// The minimum value for the incrementer
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        /// The delay before repeating starts
        /// </summary>
        public uint RepeatDelay { get; set; }

        /// <summary>
        /// The time between repeats
        /// </summary>
        public uint RepeatTime { get; set; }

        bool SignedMode;
        CTimer Timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sig">the signal toi be incremented</param>
        /// <param name="changeAmount">the amount to increment by</param>
        /// <param name="minValue">the minimum value of the signal</param>
        /// <param name="maxValue">the maximum value of the signal</param>
        /// <param name="repeatDelay">the delay before repeating starts</param>
        /// <param name="repeatTime">the time between repeats</param>
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
        /// StartUp method
        /// </summary>
        public void StartUp()
        {
            if (Timer != null) return;
            Go(ChangeAmount);
        }

        /// <summary>
        /// StartDown method
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
                Timer = new CTimer(o => { Go(change); }, null, RepeatDelay, RepeatTime);
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
        /// Stop method
        /// </summary>
        public void Stop()
        {
            if (Timer != null)
                Timer.Stop();
            Timer = null;
        }

        void SetValue(ushort value)
        {
            //CrestronConsole.PrintLine("Increment level:{0} / {1}", value, (short)value);
            TheSig.UShortValue = value;
        }
    }
}