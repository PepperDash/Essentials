

using System;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Represents a LightingScene
    /// </summary>
    public class LightingScene
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; set; }
        
        bool _IsActive;

        /// <summary>
        /// Gets or sets whether the scene is active
        /// </summary>
        [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive 
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
                IsActiveFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the SortOrder
        /// </summary>
        [JsonProperty("sortOrder", NullValueHandling = NullValueHandling.Ignore)]
        public int SortOrder { get; set; }


        /// <summary>
        /// Gets or sets the IsActiveFeedback
        /// </summary>
        [JsonIgnore]
        public BoolFeedback IsActiveFeedback { get; set; }

        /// <summary>
        /// Constructor for LightingScene
        /// </summary>
        public LightingScene()
        {
            IsActiveFeedback = new BoolFeedback(new Func<bool>(() => IsActive));
        }
    }
}