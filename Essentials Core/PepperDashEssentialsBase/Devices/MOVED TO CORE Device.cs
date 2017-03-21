//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace PepperDash.Essentials.Core
//{
//    //*********************************************************************************************************
//    /// <summary>
//    /// The core event and status-bearing class that most if not all device and connectors can derive from.
//    /// </summary>
//    public class Device : IKeyed
//    {
//        public string Key { get; protected set; }
//        public string Name { get; protected set; }
//        public bool Enabled { get; protected set; }
//        List<Action> _PreActivationActions;
//        List<Action> _PostActivationActions;

//        public static Device DefaultDevice { get { return _DefaultDevice; } }
//        static Device _DefaultDevice = new Device("Default", "Default");

//        /// <summary>
//        /// Base constructor for all Devices.
//        /// </summary>
//        /// <param name="key"></param>
//        public Device(string key)
//        {
//            Key = key;
//            if (key.Contains('.')) Debug.Console(0, this, "WARNING: Device name's should not include '.'");
//            Name = "";
//        }

//        public Device(string key, string name) : this(key)
//        {
//            Name = name;
//        }

//        public void AddPreActivationAction(Action act)
//        {
//            if (_PreActivationActions == null)
//                _PreActivationActions = new List<Action>();
//            _PreActivationActions.Add(act);
//        }

//        public void AddPostActivationAction(Action act)
//        {
//            if (_PostActivationActions == null)
//                _PostActivationActions = new List<Action>();
//            _PostActivationActions.Add(act);
//        }

//        /// <summary>
//        /// Gets this device ready to be used in the system. Runs any added pre-activation items, and
//        /// all post-activation at end. Classes needing additional logic to 
//        /// run should override CustomActivate()
//        /// </summary>
//        public bool Activate() 
//        {
//            if (_PreActivationActions != null)
//                _PreActivationActions.ForEach(a => a.Invoke());
//            var result = CustomActivate();
//            if(result && _PostActivationActions != null)
//                _PostActivationActions.ForEach(a => a.Invoke());
//            return result; 	
//        }

//        /// <summary>
//        /// Called in between Pre and PostActivationActions when Activate() is called. 
//        /// Override to provide addtitional setup when calling activation.  Overriding classes 
//        /// do not need to call base.CustomActivate()
//        /// </summary>
//        /// <returns>true if device activated successfully.</returns>
//        public virtual bool CustomActivate() { return true; }

//        /// <summary>
//        /// Call to deactivate device - unlink events, etc.  Overriding classes do not
//        /// need to call base.Deactivate()
//        /// </summary>
//        /// <returns></returns>
//        public virtual bool Deactivate() { return true; }

//        /// <summary>
//        /// Helper method to check object for bool value false and fire an Action method
//        /// </summary>
//        /// <param name="o">Should be of type bool, others will be ignored</param>
//        /// <param name="a">Action to be run when o is false</param>
//        public void OnFalse(object o, Action a)
//        {
//            if (o is bool && !(bool)o) a();
//        }
//    }
//}