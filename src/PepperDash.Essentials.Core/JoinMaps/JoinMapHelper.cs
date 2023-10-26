using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public static class JoinMapHelper
    {
        /// <summary>
        /// Attempts to get the serialized join map from config
        /// </summary>
        /// <param name="joinMapKey"></param>
        /// <returns></returns>
        public static string GetSerializedJoinMapForDevice(string joinMapKey)
        {
            if (string.IsNullOrEmpty(joinMapKey))
                return null;

            var joinMap = ConfigReader.ConfigObject.JoinMaps[joinMapKey];

            return joinMap.ToString();
        }

        /// <summary>
        /// Attempts to get the serialized join map from config
        /// </summary>
        /// <param name="joinMapKey"></param>
        /// <returns></returns>
        public static string GetJoinMapForDevice(string joinMapKey)
        {
            return GetSerializedJoinMapForDevice(joinMapKey);
        }

        /// <summary>
        /// Attempts to find a custom join map by key and returns it deserialized if found
        /// </summary>
        /// <param name="joinMapKey"></param>
        /// <returns></returns>
        public static Dictionary<string, JoinData> TryGetJoinMapAdvancedForDevice(string joinMapKey)
        {
            try
            {
                if (string.IsNullOrEmpty(joinMapKey))
                    return null;

                if (!ConfigReader.ConfigObject.JoinMaps.ContainsKey(joinMapKey))
                {
                    Debug.Console(2, "No Join Map found in config with key: '{0}'", joinMapKey);
                    return null;
                }

                Debug.Console(2, "Attempting to load custom join map with key: {0}", joinMapKey);

                var joinMapJToken = ConfigReader.ConfigObject.JoinMaps[joinMapKey];

                if (joinMapJToken == null)
                    return null;

                var joinMapData = joinMapJToken.ToObject<Dictionary<string, JoinData>>();

                return joinMapData;
            }
            catch (Exception e)
            {
                Debug.Console(2, "Error getting join map for key: '{0}'.  Error: {1}", joinMapKey, e);
                return null;
            }
        }

    }
}