using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Interfaces.Components;
using PepperDash.Essentials.Core.Room.Components;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Room
{
    /// <summary>
    /// The base config class for various component types
    /// </summary>
    public abstract class RoomComponentConfig : DeviceConfig
    {

    }

    /// <summary>
    /// The config class for an activiry
    /// </summary>
    public class RoomActivityConfig : RoomComponentConfig
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("componentKey")]
        public string ComponentKey { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
        [JsonProperty("selectAction")]
        public DeviceActionWrapper SelectAction { get; set; }
    }

    /// <summary>
    /// The config class for a room behaviour
    /// </summary>
    public class RoomBehaviourConfig : RoomComponentConfig
    {

    }

    /// <summary>
    /// The config class for a device behavior
    /// </summary>
    public class RoomDeviceBehaviourConfig : RoomComponentConfig
    {
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }
    }

    /// <summary>
    /// The config class for a ComponentRoom
    /// </summary>
    public class ComponentRoomPropertiesConfig
    {
        [JsonProperty("activities")]
        public List<RoomActivityConfig> Activities { get; set; }
        [JsonProperty("components")]
        public List<RoomComponentConfig> Components { get; set; }

    }


    /// <summary>
    /// A room comprised of component parts built at runtime.  
    /// </summary>
    public class ComponentRoom : ReconfigurableDevice, IComponentRoom
    {
        public ComponentRoomPropertiesConfig PropertiesConfig { get; private set; }

        public List<IActivatableComponent> Components { get; private set; }
        public List<IRoomActivityComponent> Activities { get; private set; }

        public ComponentRoom(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = config.Properties.ToObject<ComponentRoomPropertiesConfig>();

                BuildComponents();

                BuildActivities();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building ComponentRoom: \n{0}", e);
            }

        }


        private void BuildComponents()
        {
            foreach (var compConf in PropertiesConfig.Components)
            {
                IKeyed newComponent = null;

                newComponent = ComponentFactory.GetComponent(compConf, this);

                if (newComponent != null)
                {
                    Components.Add(newComponent as IActivatableComponent);
                    DeviceManager.AddDevice(newComponent);
                }
                else
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Unable to build room component with key: {0}", compConf.Key);
                }
            }
        }


        private void BuildActivities()
        {
            foreach (var compConf in PropertiesConfig.Activities)
            {
                IKeyed newComponent = null;

                newComponent = ComponentFactory.GetComponent(compConf, this);

                if (newComponent != null)
                {
                    Activities.Add(newComponent as IRoomActivityComponent);
                    DeviceManager.AddDevice(newComponent);
                }
                else
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Unable to build room activity with key: {0}", compConf.Key);
                }
            }
        }
        

        /// <summary>
        /// Returns a set of IRoomComponent that matches the specified Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponentsOfType<T>() where T : IActivatableComponent
        {
            return Components.OfType<T>().ToList();
        }

        /// <summary>
        /// Returns a list of the activies sorted by order
        /// </summary>
        /// <returns></returns>
        public List<IRoomActivityComponent> GetOrderedActvities()
        {
            return Activities.OrderBy(a => a.Order).ToList<IRoomActivityComponent>();
        }

    }
}