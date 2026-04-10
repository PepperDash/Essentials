

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Config;


using PepperDash.Core;
using Serilog.Events;
using System.IO;
using PepperDash.Core.Logging;

namespace PepperDash.Essentials.Core
{

	/// <summary>
	/// IR port wrapper. May act standalone
	/// </summary>
	public class IrOutputPortController : Device
	{	
		uint IrPortUid;
		IROutputPort IrPort;

		/// <summary>
		/// Gets the DriverLoaded feedback
		/// </summary>
        public BoolFeedback DriverLoaded { get; private set; }

		/// <summary>
		/// Gets or sets the StandardIrPulseTime
		/// </summary>
		public ushort StandardIrPulseTime { get; set; }

		/// <summary>
		/// Gets or sets the DriverFilepath
		/// </summary>
		public string DriverFilepath { get; private set; }

		/// <summary>
		/// Gets or sets the DriverIsLoaded
		/// </summary>
		public bool DriverIsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets the IrFileCommands
        /// </summary>
        public string[] IrFileCommands { get { return IrPort.AvailableStandardIRCmds(IrPortUid); } }

		/// <summary>
		/// Gets or sets the UseBridgeJoinMap
		/// </summary>
		public bool UseBridgeJoinMap { get; private set; }

		/// <summary>
		/// Constructor for IrDevice base class.  If a null port is provided, this class will 
		/// still function without trying to talk to a port.
		/// </summary>
		public IrOutputPortController(string key, IROutputPort port, string irDriverFilepath)
			: base(key)
		{
			//if (port == null) throw new ArgumentNullException("port");

		    DriverLoaded = new BoolFeedback(() => DriverIsLoaded);
			IrPort = port;
			if (port == null)
			{
				Debug.LogMessage(LogEventLevel.Information, this, "WARNING No valid IR Port assigned to controller. IR will not function");
				return;
			}
			LoadDriver(irDriverFilepath);
		}

		/// <summary>
		/// Constructor for IrDevice base class using post activation function to get port
		/// </summary>
		/// <param name="key">key of the device</param>
		/// <param name="postActivationFunc">function to call post activation</param>
		/// <param name="config">config of the device</param>
	    public IrOutputPortController(string key, Func<DeviceConfig, IROutputPort> postActivationFunc,
	        DeviceConfig config)
	        : base(key)
	    {
            DriverLoaded = new BoolFeedback(() => DriverIsLoaded);
			UseBridgeJoinMap = config.Properties["control"].Value<bool>("useBridgeJoinMap");
            AddPostActivationAction(() =>
            {
	            IrPort = postActivationFunc(config);

                if (IrPort == null)
                {
                    Debug.LogMessage(LogEventLevel.Information, this, "WARNING No valid IR Port assigned to controller. IR will not function");
                    return;
                }
                
                // var filePath = Global.FilePathPrefix + "ir" + Global.DirectorySeparator + config.Properties["control"]["irFile"].Value<string>();

                var fileName = config.Properties["control"]["irFile"].Value<string>();

                var files = Directory.GetFiles(Global.FilePathPrefix, fileName, SearchOption.AllDirectories);

                if(files.Length == 0)
                {
                    this.LogError("IR file {fileName} not found in {path}", fileName, Global.FilePathPrefix);
                    return;
                }

                if(files.Length > 1)
                {
                    this.LogError("IR file {fileName} found in multiple locations: {files}", fileName, files);
                    return;
                }

                var filePath = files[0];

                Debug.LogMessage(LogEventLevel.Debug, "*************Attempting to load IR file: {0}***************", filePath);

                LoadDriver(filePath);
                
				PrintAvailableCommands();
            });
	    }

		/// <summary>
		/// PrintAvailableCommands method
		/// </summary>
	    public void PrintAvailableCommands()
	    {
            Debug.LogMessage(LogEventLevel.Verbose, this, "Available IR Commands in IR File {0}", IrPortUid);
            foreach (var cmd in IrPort.AvailableIRCmds())
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "{0}", cmd);
            }
	    }
	    

	    /// <summary>
		/// Loads the IR driver at path
		/// </summary>
		/// <param name="path">path of the IR driver file</param>
		public void LoadDriver(string path)
		{
            Debug.LogMessage(LogEventLevel.Verbose, this, "***Loading IR File***");
			if (string.IsNullOrEmpty(path)) path = DriverFilepath;
	        try
	        {
	            IrPortUid = IrPort.LoadIRDriver(path);
	            DriverFilepath = path;
	            StandardIrPulseTime = 200;
	            DriverIsLoaded = true;

                DriverLoaded.FireUpdate();
	        }
			catch
			{
				DriverIsLoaded = false;
				var message = string.Format("WARNING IR Driver '{0}' failed to load", path);
				Debug.LogMessage(LogEventLevel.Information, this, message);
                DriverLoaded.FireUpdate();
			}
		}


		/// <summary>
		/// PressRelease method
		/// </summary>
		/// <param name="command">IR command to send</param>
		/// <param name="state">true to press, false to release</param>
		/// <inheritdoc />
		public virtual void PressRelease(string command, bool state)
		{
			Debug.LogMessage(LogEventLevel.Verbose, this, "IR:'{0}'={1}", command, state);
			if (IrPort == null)
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "WARNING No IR Port assigned to controller");
				return;
			} 
			if (!DriverIsLoaded)
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "WARNING IR driver is not loaded");
				return;
			}
			if (state)
			{
				if (IrPort.IsIRCommandAvailable(IrPortUid, command))
					IrPort.Press(IrPortUid, command);
				else
					NoIrCommandError(command);
			}
			else
				IrPort.Release();
		}

		/// <summary>
		/// Pulse method
		/// </summary>
		/// <param name="command">IR command to send</param>
		/// <param name="time">time to pulse the command</param>
		/// <inheritdoc />
		public virtual void Pulse(string command, ushort time)
		{
			if (IrPort == null)
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "WARNING No IR Port assigned to controller");
				return;
			} 
			if (!DriverIsLoaded)
			{
				Debug.LogMessage(LogEventLevel.Verbose, this, "WARNING IR driver is not loaded");
				return;
			}
			if (IrPort.IsIRCommandAvailable(IrPortUid, command))
				IrPort.PressAndRelease(IrPortUid, command, time);
			else
				NoIrCommandError(command);
		}

		/// <summary>
		/// Notifies the console when a bad command is used.
		/// </summary>
		/// <param name="command">command that was not found</param>
		protected void NoIrCommandError(string command)
		{
			Debug.LogMessage(LogEventLevel.Verbose, this, "Device {0}: IR Driver {1} does not contain command {2}",
				Key, IrPort.IRDriverFileNameByIRDriverId(IrPortUid), command);
		}
	}
}