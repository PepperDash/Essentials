using System;
using System.Collections.Generic;
using Serilog.Events;

namespace PepperDash.Core
{
	//*********************************************************************************************************
	/// <summary>
	/// The core event and status-bearing class that most if not all device and connectors can derive from.
	/// </summary>
	public class Device : IKeyName
	{

        /// <summary>
        /// Unique Key
        /// </summary>
		public string Key { get; protected set; }
		/// <summary>
		/// Name of the devie
		/// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
		public bool Enabled { get; protected set; }

        ///// <summary>
        ///// A place to store reference to the original config object, if any. These values should 
        ///// NOT be used as properties on the device as they are all publicly-settable values.
        ///// </summary>
        //public DeviceConfig Config { get; private set; }
        ///// <summary>
        ///// Helper method to check if Config exists
        ///// </summary>
        //public bool HasConfig { get { return Config != null; } }

		List<Action> _PreActivationActions;
		List<Action> _PostActivationActions;

        /// <summary>
        /// 
        /// </summary>
		public static Device DefaultDevice { get { return _DefaultDevice; } }
		static Device _DefaultDevice = new Device("Default", "Default");

		/// <summary>
		/// Base constructor for all Devices.
		/// </summary>
		/// <param name="key"></param>
		public Device(string key)
		{
			Key = key;
			if (key.Contains(".")) Debug.LogMessage(LogEventLevel.Information, "WARNING: Device key should not include '.'", this);
			Name = "";
		}

        /// <summary>
        /// Constructor with key and name
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
		public Device(string key, string name) : this(key)
		{
			Name = name;

		}

        //public Device(DeviceConfig config)
        //    : this(config.Key, config.Name)
        //{
        //    Config = config;
        //}

        /// <summary>
        /// Adds a pre activation action
        /// </summary>
        /// <param name="act"></param>
		public void AddPreActivationAction(Action act)
		{
			if (_PreActivationActions == null)
				_PreActivationActions = new List<Action>();
			_PreActivationActions.Add(act);
		}

        /// <summary>
        /// Adds a post activation action
        /// </summary>
        /// <param name="act"></param>
		public void AddPostActivationAction(Action act)
		{
			if (_PostActivationActions == null)
				_PostActivationActions = new List<Action>();
			_PostActivationActions.Add(act);
		}

        /// <summary>
        /// Executes the preactivation actions
        /// </summary>
        public void PreActivate()
        {
            if (_PreActivationActions != null)
                _PreActivationActions.ForEach(a => {
					try
					{
						a.Invoke();
					} catch (Exception e)
					{ 
						Debug.LogMessage(e, "Error in PreActivationAction: " + e.Message, this);
					}
            });
        }

		/// <summary>
		/// Gets this device ready to be used in the system. Runs any added pre-activation items, and
		/// all post-activation at end. Classes needing additional logic to 
		/// run should override CustomActivate()
		/// </summary>
		public bool Activate() 
		{
            //if (_PreActivationActions != null)
            //    _PreActivationActions.ForEach(a => a.Invoke());
			var result = CustomActivate();
            //if(result && _PostActivationActions != null)
            //    _PostActivationActions.ForEach(a => a.Invoke());
			return result; 	
		}

        /// <summary>
        /// Executes the postactivation actions
        /// </summary>
        public void PostActivate()
        {
            if (_PostActivationActions != null)
                _PostActivationActions.ForEach(a => {
                    try
                    {
                        a.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(e, "Error in PostActivationAction: " + e.Message, this);
                    }
                });
        }

		/// <summary>
		/// Called in between Pre and PostActivationActions when Activate() is called. 
		/// Override to provide addtitional setup when calling activation.  Overriding classes 
		/// do not need to call base.CustomActivate()
		/// </summary>
		/// <returns>true if device activated successfully.</returns>
		public virtual bool CustomActivate() { return true; }

		/// <summary>
		/// Call to deactivate device - unlink events, etc.  Overriding classes do not
		/// need to call base.Deactivate()
		/// </summary>
		/// <returns></returns>
		public virtual bool Deactivate() { return true; }

        /// <summary>
        /// Call this method to start communications with a device. Overriding classes do not need to call base.Initialize()
        /// </summary>
	    public virtual void Initialize()
	    {
	    }

	    /// <summary>
		/// Helper method to check object for bool value false and fire an Action method
		/// </summary>
		/// <param name="o">Should be of type bool, others will be ignored</param>
		/// <param name="a">Action to be run when o is false</param>
		public void OnFalse(object o, Action a)
		{
			if (o is bool && !(bool)o) a();
		}

	}
}