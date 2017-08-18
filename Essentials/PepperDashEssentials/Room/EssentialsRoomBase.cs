using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHasCurrentSourceInfoChange
    {
        event SourceInfoChangeHandler CurrentSingleSourceChange;
    }


    /// <summary>
    /// 
    /// </summary>
    public abstract class EssentialsRoomBase : Device
    {
        /// <summary>
        ///
        /// </summary>
        public BoolFeedback OnFeedback { get; private set; }

        public BoolFeedback IsWarmingFeedback { get; private set; }
        public BoolFeedback IsCoolingFeedback { get; private set; }
        public BoolFeedback ShutdownPendingFeedback { get; private set; }

        protected abstract Func<bool> OnFeedbackFunc { get; }


        public EssentialsRoomBase(string key, string name) : base(key, name)
        {
            OnFeedback = new BoolFeedback(OnFeedbackFunc);
        }
    }
}