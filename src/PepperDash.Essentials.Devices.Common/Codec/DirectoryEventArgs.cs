using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// 
    /// </summary>
    public class DirectoryEventArgs : EventArgs
    {
        public CodecDirectory Directory { get; set; }
        public bool DirectoryIsOnRoot { get; set; }
    }
}