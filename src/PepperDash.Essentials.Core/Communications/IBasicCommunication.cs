namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// Defines the contract for IBasicCommunication
    /// </summary>
    public interface IBasicCommunication : ICommunicationReceiver
	{
        /// <summary>
        /// Send text to the device
        /// </summary>
        /// <param name="text"></param>
		void SendText(string text);

        /// <summary>
        /// Send bytes to the device
        /// </summary>
        /// <param name="bytes"></param>
		void SendBytes(byte[] bytes);
	}
}