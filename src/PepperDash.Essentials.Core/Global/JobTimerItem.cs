using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class JobTimerItem
    {
        public string Key { get; private set; }
        public Action JobAction { get; private set; }
        public eJobTimerCycleTypes CycleType { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime RunNextAt { get; set; }

        public JobTimerItem(string key, eJobTimerCycleTypes cycle, Action act)
        {

        }
    }
}