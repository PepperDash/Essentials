namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Requirements for a device that implements basic Open/Close/Stop shade control (Uses 3 relays)
    /// </summary>
    public interface IShadesOpenCloseStop
    {
        void Open();
        void Close();
        void Stop();
    }
}