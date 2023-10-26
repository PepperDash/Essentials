using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// 
    /// </summary>
    public class CodecCallStatusItemChangeEventArgs : EventArgs
    {
        public CodecActiveCallItem CallItem { get; private set; }

        public CodecCallStatusItemChangeEventArgs(CodecActiveCallItem item)
        {
            CallItem = item;
        }
    }
}