using System;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Requirements for a device that implements basic Open/Close shade control
    /// </summary>
    [Obsolete("Please use IShadesOpenCloseStop instead")]
    public interface IShadesOpenClose
    {
        void Open();
        void Close();
    }
}