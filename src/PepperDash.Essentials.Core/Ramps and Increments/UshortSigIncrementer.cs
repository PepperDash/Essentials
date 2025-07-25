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
        public ushort ChangeAmount { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public uint RepeatDelay { get; set; }
        public uint RepeatTime { get; set; }
        bool SignedMode;
        CTimer Timer;

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