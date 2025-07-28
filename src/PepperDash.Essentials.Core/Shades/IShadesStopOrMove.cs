namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Used to implement raise/stop/lower/stop from single button
    /// </summary>
    public interface IShadesStopOrMove
	{
		void OpenOrStop();
		void CloseOrStop();
		void OpenCloseOrStop();
	}
}