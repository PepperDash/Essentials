using System;

namespace PepperDash.Essentials.Core.Communications
{

	/// <summary>
	/// 
	/// </summary>
	public enum eGenericCommMethodStatusChangeType
	{
        /// <summary>
        /// Connected
        /// </summary>
		Connected,
        /// <summary>
        /// Disconnected
        /// </summary>
        Disconnected
	}

	/// <summary>
	/// This delegate defines handler for IBasicCommunication status changes
	/// </summary>
	/// <param name="comm">Device firing the status change</param>
	/// <param name="status"></param>
	public delegate void GenericCommMethodStatusHandler(IBasicCommunication comm, eGenericCommMethodStatusChangeType status);

	/// <summary>
	/// 
	/// </summary>
	public class GenericCommMethodReceiveBytesArgs : EventArgs
	{
  /// <summary>
  /// Gets or sets the Bytes
  /// </summary>
		public byte[] Bytes { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
		public GenericCommMethodReceiveBytesArgs(byte[] bytes)
		{
			Bytes = bytes;
		}

		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericCommMethodReceiveBytesArgs() { }
	}

	/// <summary>
	/// 
	/// </summary>
	public class GenericCommMethodReceiveTextArgs : EventArgs
	{
        /// <summary>
        /// 
        /// </summary>
		public string Text { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string Delimiter { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
		public GenericCommMethodReceiveTextArgs(string text)
		{
			Text = text;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="delimiter"></param>
        public GenericCommMethodReceiveTextArgs(string text, string delimiter)
            :this(text)
        {
            Delimiter = delimiter;
        }

		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericCommMethodReceiveTextArgs() { }
	}
}