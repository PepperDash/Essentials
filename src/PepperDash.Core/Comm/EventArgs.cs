/*PepperDash Technology Corp.
Copyright:		2017
------------------------------------
***Notice of Ownership and Copyright***
The material in which this notice appears is the property of PepperDash Technology Corporation, 
which claims copyright under the laws of the United States of America in the entire body of material 
and in all parts thereof, regardless of the use to which it is being put.  Any use, in whole or in part, 
of this material by another party without the express written permission of PepperDash Technology Corporation is prohibited.  
PepperDash Technology Corporation reserves all rights under applicable laws.
------------------------------------ */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;


namespace PepperDash.Core
{
    /// <summary>
    /// Delegate for notifying of socket status changes
    /// </summary>
    /// <param name="client"></param>
    public delegate void GenericSocketStatusChangeEventDelegate(ISocketStatus client);

    /// <summary>
    /// EventArgs class for socket status changes
    /// </summary>
	public class GenericSocketStatusChageEventArgs : EventArgs
	{
        /// <summary>
        /// 
        /// </summary>
		public ISocketStatus Client { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
		public GenericSocketStatusChageEventArgs(ISocketStatus client)
		{
			Client = client;
		}
		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericSocketStatusChageEventArgs() { }
    }

    /// <summary>
    /// Delegate for notifying of TCP Server state changes
    /// </summary>
    /// <param name="state"></param>
    public delegate void GenericTcpServerStateChangedEventDelegate(ServerState state);

    /// <summary>
    /// EventArgs class for TCP Server state changes
    /// </summary>
    public class GenericTcpServerStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public ServerState State { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public GenericTcpServerStateChangedEventArgs(ServerState state)
        {
            State = state;
        }
		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericTcpServerStateChangedEventArgs() { }
    }

    /// <summary>
    /// Delegate for TCP Server socket status changes
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="clientIndex"></param>
    /// <param name="clientStatus"></param>
    public delegate void GenericTcpServerSocketStatusChangeEventDelegate(object socket, uint clientIndex, SocketStatus clientStatus);
    /// <summary>
    /// EventArgs for TCP server socket status changes
    /// </summary>
    public class GenericTcpServerSocketStatusChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public object Socket { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public uint ReceivedFromClientIndex { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public SocketStatus ClientStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="clientStatus"></param>
        public GenericTcpServerSocketStatusChangeEventArgs(object socket, SocketStatus clientStatus)
        {
            Socket = socket;
            ClientStatus = clientStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="clientIndex"></param>
        /// <param name="clientStatus"></param>
        public GenericTcpServerSocketStatusChangeEventArgs(object socket, uint clientIndex, SocketStatus clientStatus)
        {
            Socket = socket;
            ReceivedFromClientIndex = clientIndex;
            ClientStatus = clientStatus;
        }
		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericTcpServerSocketStatusChangeEventArgs() { }
    }

    /// <summary>
    /// EventArgs for TCP server com method receive text
    /// </summary>
    public class GenericTcpServerCommMethodReceiveTextArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public uint ReceivedFromClientIndex { get; private set; }

        /// <summary>
        /// 
        /// </summary>
		public ushort ReceivedFromClientIndexShort
		{
			get
			{
				return (ushort)ReceivedFromClientIndex;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public GenericTcpServerCommMethodReceiveTextArgs(string text)
        {
            Text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="clientIndex"></param>
        public GenericTcpServerCommMethodReceiveTextArgs(string text, uint clientIndex)
        {
            Text = text;
            ReceivedFromClientIndex = clientIndex;
        }
		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericTcpServerCommMethodReceiveTextArgs() { }
    }

    /// <summary>
    /// EventArgs for TCP server client ready for communication
    /// </summary>
    public class GenericTcpServerClientReadyForcommunicationsEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsReady;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReady"></param>
        public GenericTcpServerClientReadyForcommunicationsEventArgs(bool isReady)
        {
            IsReady = isReady;
        }
		/// <summary>
		/// S+ Constructor
		/// </summary>
		public GenericTcpServerClientReadyForcommunicationsEventArgs() { }
    }

    /// <summary>
    /// EventArgs for UDP connected
    /// </summary>
    public class GenericUdpConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public ushort UConnected;
        /// <summary>
        /// 
        /// </summary>
        public bool Connected;

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericUdpConnectedEventArgs() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uconnected"></param>
        public GenericUdpConnectedEventArgs(ushort uconnected)
        {
            UConnected = uconnected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connected"></param>
        public GenericUdpConnectedEventArgs(bool connected)
        {
            Connected = connected;
        }

    }

   

}