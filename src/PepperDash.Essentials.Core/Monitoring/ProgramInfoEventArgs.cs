using System;

namespace PepperDash.Essentials.Core.Monitoring
{
    public class ProgramInfoEventArgs : EventArgs
    {
        public ProgramInfo ProgramInfo;

        public ProgramInfoEventArgs(ProgramInfo progInfo)
        {
            ProgramInfo = progInfo;
        }
    }
}