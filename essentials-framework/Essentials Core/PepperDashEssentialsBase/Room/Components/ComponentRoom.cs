using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Interfaces.Components;
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

        public List<IRoomComponent> Components { get; private set; }
        public List<IRoomActivityComponent> Activities { get; private set; }

        public ComponentRoom(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<ComponentRoomPropertiesConfig>
                    (config.Properties.ToString());
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building ComponentRoom: \n{0}", e);
            }

        }

        public List<IRoomComponent> GetRoomComponentsOfType(Type componentType)
        {
            // TODO: Figure this out later
            return Components;
            //var results = Components.OfType<componentType>();
            //return results;
            //return Components.Where(c => c != null && type.IsAssignableFrom(c.GetType()));
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