using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PepperDash.Core
{
    /// <summary>
    /// An incoming communication stream
    /// </summary>
    public interface ICommunicationReceiver : IKeyed
    {
        /// <summary>
        /// Notifies of bytes received
        /// </summary>
        event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
        /// <summary>
        /// Notifies of text received
        /// </summary>
        event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// Indicates connection status
        /// </summary>
        [JsonProperty("isConnected")]
        bool IsConnected { get; }
        /// <summary>
        /// Connect to the device
        /// </summary>
        void Connect();
        /// <summary>
        /// Disconnect from the device
        /// </summary>
        void Disconnect();
    }

	/// <summary>
	/// Represents a device that uses basic connection
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

    /// <summary>
    /// Represents a device that implements IBasicCommunication and IStreamDebugging
    /// </summary>
    public interface IBasicCommunicationWithStreamDebugging : IBasicCommunication, IStreamDebugging
    {

    }

    /// <summary>
    /// Represents a device with stream debugging capablities
    /// </summary>
    public interface IStreamDebugging
    {
        /// <summary>
        /// Object to enable stream debugging
        /// </summary>
        [JsonProperty("streamDebugging")]
        CommunicationStreamDebugging StreamDebugging { get; }
    }

	/// <summary>
	/// For IBasicCommunication classes that have SocketStatus. GenericSshClient,
	/// GenericTcpIpClient
	/// </summary>
	public interface ISocketStatus : IBasicCommunication
	{
        /// <summary>
        /// Notifies of socket status changes
        /// </summary>
		event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

        /// <summary>
        /// The current socket status of the client
        /// </summary>
        [JsonProperty("clientStatus")]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        SocketStatus ClientStatus { get; }
	}

    /// <summary>
    /// Describes a device that implements ISocketStatus and IStreamDebugging
    /// </summary>
    public interface ISocketStatusWithStreamDebugging : ISocketStatus, IStreamDebugging
    {

    }

    /// <summary>
    /// Describes a device that can automatically attempt to reconnect
    /// </summary>
	public interface IAutoReconnect
	{
        /// <summary>
        /// Enable automatic recconnect
        /// </summary>
        [JsonProperty("autoReconnect")]
		bool AutoReconnect { get; set; }
        /// <summary>
        /// Interval in ms to attempt automatic recconnections
        /// </summary>
        [JsonProperty("autoReconnectIntervalMs")]
		int AutoReconnectIntervalMs { get; set; }
	}

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
        /// 
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



	/// <summary>
	/// 
	/// </summary>
	public class ComTextHelper
	{
        /// <summary>
        /// Gets escaped text for a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
		public static string GetEscapedText(byte[] bytes)
		{
			return String.Concat(bytes.Select(b => string.Format(@"[{0:X2}]", (int)b)).ToArray());
		}

        /// <summary>
        /// Gets escaped text for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
		public static string GetEscapedText(string text)
		{
			var bytes = Encoding.GetEncoding(28591).GetBytes(text);
			return String.Concat(bytes.Select(b => string.Format(@"[{0:X2}]", (int)b)).ToArray());
		}

        /// <summary>
        /// Gets debug text for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetDebugText(string text)
        {
            return Regex.Replace(text, @"[^\u0020-\u007E]", a => GetEscapedText(a.Value));
        }
	}
}