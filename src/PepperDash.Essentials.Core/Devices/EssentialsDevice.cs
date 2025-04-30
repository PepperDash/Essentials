using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using System.Reflection;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the basic needs for an EssentialsDevice to enable it to be build by an IDeviceFactory class
    /// </summary>
    [Description("The base Essentials Device Class")]
    public abstract class EssentialsDevice : Device
    {
        public event EventHandler Initialized;

        private bool _isInitialized;
        public bool IsInitialized { 
            get { return _isInitialized; }
            private set 
            { 
                if (_isInitialized == value) return;
                
                _isInitialized = value;

                if (_isInitialized)
                {
                    Initialized?.Invoke(this, new EventArgs());
                }
            } 
        }

        protected EssentialsDevice(string key)
            : base(key)
        {
            SubscribeToActivateComplete();
        }

        protected EssentialsDevice(string key, string name)
            : base(key, name)
        {
            SubscribeToActivateComplete();
        }

        private void SubscribeToActivateComplete()
        {
            DeviceManager.AllDevicesActivated += DeviceManagerOnAllDevicesActivated;
        }

        private void DeviceManagerOnAllDevicesActivated(object sender, EventArgs eventArgs)
        {
            CrestronInvoke.BeginInvoke((o) =>
            {
                try
                {
                    Initialize();

                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(LogEventLevel.Error, this, "Exception initializing device: {0}", ex.Message);
                    Debug.LogMessage(LogEventLevel.Debug, this, "Stack Trace: {0}", ex.StackTrace);
                }
            });
        }

        public override bool CustomActivate()
        {
            CreateMobileControlMessengers();

            return base.CustomActivate();
        }

        /// <summary>
        /// Override this method to build and create custom Mobile Control Messengers during the Activation phase
        /// </summary>
        protected virtual void CreateMobileControlMessengers() {
           
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class DescriptionAttribute : Attribute
    {
        private string _Description;

        public DescriptionAttribute(string description)
        {
            //Debug.LogMessage(LogEventLevel.Verbose, "Setting Description: {0}", description);
            _Description = description;
        }

        public string Description
        {
            get { return _Description; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ConfigSnippetAttribute : Attribute
    {
        private string _ConfigSnippet;

        public ConfigSnippetAttribute(string configSnippet)
        {
            //Debug.LogMessage(LogEventLevel.Verbose, "Setting Config Snippet {0}", configSnippet);
            _ConfigSnippet = configSnippet;
        }

        public string ConfigSnippet
        {
            get { return _ConfigSnippet; }
        }
    }

    /// <summary>
    /// Devices the basic needs for a Device Factory
    /// </summary>
    public abstract class EssentialsDeviceFactory<T> : IDeviceFactory where T:EssentialsDevice
    {
        #region IDeviceFactory Members

        /// <summary>
        /// A list of strings that can be used in the type property of a DeviceConfig object to build an instance of this device
        /// </summary>
        public List<string> TypeNames { get; protected set; }

        /// <summary>
        /// Loads an item to the DeviceFactory.FactoryMethods dictionary for each entry in the TypeNames list
        /// </summary>
        public void LoadTypeFactories()
        {
            foreach (var typeName in TypeNames)
            {
                //Debug.LogMessage(LogEventLevel.Verbose, "Getting Description Attribute from class: '{0}'", typeof(T).FullName);
                var descriptionAttribute = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
                string description = descriptionAttribute[0].Description;
                var snippetAttribute = typeof(T).GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
                DeviceFactory.AddFactoryForType(typeName.ToLower(), description, typeof(T), BuildDevice);
            }
        }

        /// <summary>
        /// The method that will build the device
        /// </summary>
        /// <param name="dc">The device config</param>
        /// <returns>An instance of the device</returns>
        public abstract EssentialsDevice BuildDevice(DeviceConfig dc);

        #endregion
    }

    public abstract class ProcessorExtensionDeviceFactory<T> : IProcessorExtensionDeviceFactory where T: EssentialsDevice
    {
        #region IProcessorExtensionDeviceFactory Members

        /// <summary>
        /// A list of strings that can be used in the type property of a DeviceConfig object to build an instance of this device
        /// </summary>
        public List<string> TypeNames { get; protected set; }

        /// <summary>
        /// Loads an item to the ProcessorExtensionDeviceFactory.ProcessorExtensionFactoryMethods dictionary for each entry in the TypeNames list
        /// </summary>
        public void LoadFactories()
        {
            foreach (var typeName in TypeNames)
            {
                //Debug.LogMessage(LogEventLevel.Verbose, "Getting Description Attribute from class: '{0}'", typeof(T).FullName);
                var descriptionAttribute = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
                string description = descriptionAttribute[0].Description;
                var snippetAttribute = typeof(T).GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
                ProcessorExtensionDeviceFactory.AddFactoryForType(typeName.ToLower(), description, typeof(T), BuildDevice);
            }
        }

        /// <summary>
        /// The method that will build the device
        /// </summary>
        /// <param name="dc">The device config</param>
        /// <returns>An instance of the device</returns>
        public abstract EssentialsDevice BuildDevice(DeviceConfig dc);

        #endregion

    }

    /// <summary>
    /// Devices the basic needs for a Device Factory
    /// </summary>
    public abstract class EssentialsPluginDeviceFactory<T> : EssentialsDeviceFactory<T>, IPluginDeviceFactory where T : EssentialsDevice
    {

#if ESSENTIALS_VERSION
        /// <summary>
        /// Specifies the minimum version of Essentials required for a plugin to run.  Must use the format Major.Minor.Build (ex. "1.4.33")
        /// </summary>
        public string MinimumEssentialsFrameworkVersion { get; protected set; } = ESSENTIALS_VERSION
#else
        /// <summary>
        /// Specifies the minimum version of Essentials required for a plugin to run.  Must use the format Major.Minor.Build (ex. "1.4.33")
        /// </summary>
        public string MinimumEssentialsFrameworkVersion { get; protected set; }
#endif
    }

    public abstract class EssentialsPluginDevelopmentDeviceFactory<T> : EssentialsDeviceFactory<T>, IPluginDevelopmentDeviceFactory where T : EssentialsDevice
    {
        /// <summary>
        /// Specifies the minimum version of Essentials required for a plugin to run.  Must use the format Major.Minor.Build (ex. "1.4.33")
        /// </summary>
        public string MinimumEssentialsFrameworkVersion { get; protected set; }

        public List<string>  DevelopmentEssentialsFrameworkVersions { get; protected set; }
    }

}