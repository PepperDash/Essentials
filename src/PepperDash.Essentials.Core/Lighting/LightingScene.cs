

using System;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Represents a LightingScene
    /// </summary>
    public class LightingScene
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string ID { get; set; }
        bool _IsActive;
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

        [JsonProperty("sortOrder", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the SortOrder
        /// </summary>
        public int SortOrder { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Gets or sets the IsActiveFeedback
        /// </summary>
        public BoolFeedback IsActiveFeedback { get; set; }

        public LightingScene()
        {
            IsActiveFeedback = new BoolFeedback(new Func<bool>(() => IsActive));
        }
    }
}