using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// An incrementer that can use the values of some other object/primitive value to do its thing.
    /// It uses an Action to set the value and a Func to get the value from whatever this is
    /// attached to.
    /// </summary>
    public class ActionIncrementer
    {
        /// <summary>
        /// The amount to change the value by each increment
        /// </summary>
        public int ChangeAmount { get; set; }

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

        Action<int> SetAction;
        Func<int> GetFunc;
        CTimer Timer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changeAmount"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="repeatDelay"></param>
        /// <param name="repeatTime"></param>
        /// <param name="setAction">Action that will be called when this needs to set the destination value</param>
        /// <param name="getFunc">Func that is called to get the current value</param>
        public ActionIncrementer(int changeAmount, int minValue, int maxValue, uint repeatDelay, uint repeatTime, Action<int> setAction, Func<int> getFunc)
        {
            SetAction = setAction;
            GetFunc = getFunc;
            ChangeAmount = changeAmount;
            MaxValue = maxValue;
            MinValue = minValue;
            RepeatDelay = repeatDelay;
            RepeatTime = repeatTime;
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
        /// Starts decrementing cycle
        /// </summary>
        public void StartDown()
        {
            if (Timer != null) return;
            Go(-ChangeAmount);
        }

        /// <summary>
        /// Stops the repeat
        /// </summary>
        public void Stop()
        {
            if (Timer != null)
                Timer.Stop();
            Timer = null;
        }

        /// <summary>
        /// Helper that does the work of setting new level, and starting repeat loop, checking against bounds first.
        /// </summary>
        /// <param name="change"></param>
        void Go(int change)
        {
            int currentLevel = GetFunc();
            // Fire once then pause
            int newLevel = currentLevel + change;
            bool atLimit = CheckLevel(newLevel, out newLevel);
            SetAction(newLevel);

            if (atLimit) // Don't go past end
                Stop();
            else if (Timer == null) // Only enter the timer if it's not already running
                Timer = new CTimer(o => { Go(change); }, null, RepeatDelay, RepeatTime);
        }

        /// <summary>
        /// Helper to check a new level against min/max. Returns revised level if new level
        /// will go out of bounds
        /// </summary>
        /// <param name="levelIn">The level to check against bounds</param>
        /// <param name="levelOut">Revised level if bounds are exceeded. Min or max</param>
        /// <returns>true if new level is at or past bounds</returns>
        bool CheckLevel(int levelIn, out int levelOut)
        {
            bool isAtLimit = false;
            if (levelIn > MaxValue)
            {
                levelOut = MaxValue;
                isAtLimit = true;
            }
            else if (levelIn < MinValue)
            {
                levelOut = MinValue;
                isAtLimit = true;
            }
            else
                levelOut = levelIn;
            return isAtLimit;
        }
    }
}