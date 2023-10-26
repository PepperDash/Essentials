using System;

namespace PepperDash.Essentials
{
    public class StatusChangedEventArgs : EventArgs
    {
        public uint PreviousJoin { get; set; }
        public uint NewJoin { get; set; }
        public bool WasShown { get; set; }
        public bool IsShown  { get; set; }

        public StatusChangedEventArgs(uint prevJoin, uint newJoin, bool wasShown, bool isShown)
        {
            PreviousJoin = prevJoin;
            NewJoin      = newJoin;
            WasShown     = wasShown;
            IsShown      = isShown;
        }
    }
}