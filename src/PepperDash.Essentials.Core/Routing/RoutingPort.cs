using System;
using System.Collections.Generic;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Base class for RoutingInput and Output ports
	/// </summary>
	public abstract class RoutingPort : IKeyed
	{
		public string Key { get; private set; }
		public eRoutingSignalType Type { get; private set; }
		public eRoutingPortConnectionType ConnectionType { get; private set; }
		public readonly object Selector;
		public bool IsInternal { get; private set; }
        public object FeedbackMatchObject { get; set; }
        public object Port { get; set; }

		public RoutingPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType, object selector, bool isInternal)
		{
			Key = key;
			Type = type;
			ConnectionType = connType;
			Selector = selector;
			IsInternal = IsInternal;
		}
	}

    [Flags]
	public enum eRoutingSignalType
	{
		Audio = 1,
		Video = 2,     
		AudioVideo = Audio | Video,
        UsbOutput = 8,
        UsbInput = 16
	}

	public enum eRoutingPortConnectionType
	{
		None, BackplaneOnly, DisplayPort, Dvi, Hdmi, Rgb, Vga, LineAudio, DigitalAudio, Sdi, 
		Composite, Component, DmCat, DmMmFiber, DmSmFiber, Speaker, Streaming
	}

	/// <summary>
	/// Basic RoutingInput with no statuses.
	/// </summary>
	public class RoutingInputPort : RoutingPort
	{
		/// <summary>
		/// The IRoutingInputs object this lives on
		/// </summary>
		public IRoutingInputs ParentDevice { get; private set; }

		/// <summary>
		/// Constructor for a basic RoutingInputPort
		/// </summary>
		/// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
		/// May be string, number, whatever</param>
		/// <param name="parent">The IRoutingInputs object this lives on</param>
		public RoutingInputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
			object selector, IRoutingInputs parent)
			: this (key, type, connType, selector, parent, false)
		{
		}

		/// <summary>
		/// Constructor for a virtual routing input port that lives inside a device. For example
		/// the ports that link a DM card to a DM matrix bus
		/// </summary>
		/// <param name="isInternal">true for internal ports</param>
		public RoutingInputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
			object selector, IRoutingInputs parent, bool isInternal)
			: base(key, type, connType, selector, isInternal)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");
			ParentDevice = parent;
		}




		///// <summary>
		///// Static method to get a named port from a named device
		///// </summary>
		///// <returns>Returns null if device or port doesn't exist</returns>
		//public static RoutingInputPort GetDevicePort(string deviceKey, string portKey)
		//{
		//    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as IRoutingInputs;
		//    if (sourceDev == null)
		//        return null;
		//    return sourceDev.InputPorts[portKey];
		//}

		///// <summary>
		///// Static method to get a named port from a card in a named ICardPortsDevice device
		///// Uses ICardPortsDevice.GetChildInputPort 
		///// </summary>
		///// <param name="cardKey">'input-N'</param>
		///// <returns>null if device, card or port doesn't exist</returns>
		//public static RoutingInputPort GetDeviceCardPort(string deviceKey, string cardKey, string portKey)
		//{
		//    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as ICardPortsDevice;
		//    if (sourceDev == null)
		//        return null;
		//    return sourceDev.GetChildInputPort(cardKey, portKey);
		//}
	}

	/// <summary>
	/// A RoutingInputPort for devices like DM-TX and DM input cards. 
	/// Will provide video statistics on connected signals
	/// </summary>
	public class RoutingInputPortWithVideoStatuses : RoutingInputPort
	{
		/// <summary>
		/// Video statuses attached to this port
		/// </summary>
		public VideoStatusOutputs VideoStatus { get; private set; }

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
		/// May be string, number, whatever</param>
		/// <param name="parent">The IRoutingInputs object this lives on</param>
		/// <param name="funcs">A VideoStatusFuncsWrapper used to assign the callback funcs that will get 
		/// the values for the various stats</param>
		public RoutingInputPortWithVideoStatuses(string key, 
			eRoutingSignalType type, eRoutingPortConnectionType connType, object selector, 
			IRoutingInputs parent, VideoStatusFuncsWrapper funcs) :
			base(key, type, connType, selector, parent)
		{
			VideoStatus = new VideoStatusOutputs(funcs);		
		}
	}

	public class RoutingOutputPort : RoutingPort
	{
		/// <summary>
		/// The IRoutingOutputs object this port lives on
		/// </summary>
		public IRoutingOutputs ParentDevice { get; private set; }

		public InUseTracking InUseTracker { get; private set; }


		/// <summary>
		/// </summary>
		/// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
		/// May be string, number, whatever</param>
		/// <param name="parent">The IRoutingOutputs object this port lives on</param>
		public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
			object selector, IRoutingOutputs parent)
			: this(key, type, connType, selector, parent, false)
		{
		}

		public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
			object selector, IRoutingOutputs parent, bool isInternal)
			: base(key, type, connType, selector, isInternal)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");
			ParentDevice = parent;
			InUseTracker = new InUseTracking();
		}

		public override string ToString()
		{
			return ParentDevice.Key + ":" + Key;
		}

		///// <summary>
		///// Static method to get a named port from a named device
		///// </summary>
		///// <returns>Returns null if device or port doesn't exist</returns>
		//public static RoutingOutputPort GetDevicePort(string deviceKey, string portKey)
		//{
		//    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as IRoutingOutputs;
		//    if (sourceDev == null)
		//        return null;
		//    var port = sourceDev.OutputPorts[portKey];
		//    if (port == null)
		//        Debug.Console(0, "WARNING: Device '{0}' does does not contain output port '{1}'", deviceKey, portKey);
		//    return port;
		//}

		///// <summary>
		///// Static method to get a named port from a card in a named ICardPortsDevice device
		///// Uses ICardPortsDevice.GetChildOutputPort on that device
		///// </summary>
		///// <param name="cardKey">'input-N' or 'output-N'</param>
		///// <returns>null if device, card or port doesn't exist</returns>
		//public static RoutingOutputPort GetDeviceCardPort(string deviceKey, string cardKey, string portKey)
		//{
		//    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as ICardPortsDevice;
		//    if (sourceDev == null)
		//        return null;
		//    var port = sourceDev.GetChildOutputPort(cardKey, portKey);
		//}
	}
}