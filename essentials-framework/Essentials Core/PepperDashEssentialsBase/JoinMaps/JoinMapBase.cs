using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
        public static string GetJoinMapForDevice(string joinMapKey)
        {
            if (string.IsNullOrEmpty(joinMapKey))
                return null;

            var joinMap = ConfigReader.ConfigObject.JoinMaps[joinMapKey];

            if (joinMap != null)
            {
                return joinMap;
            }
            else
                return null;
        }
    }


    public abstract class JoinMapBase
    {
        /// <summary>
        /// Modifies all the join numbers by adding the offset.  This should never be called twice
        /// </summary>
        /// <param name="joinStart"></param>
        public abstract void OffsetJoinNumbers(uint joinStart);

        public Dictionary<string, JoinMetadata> Joins { get; set; }
    }

    public enum eJoinCapabilities
    {
        Read = 1,
        Write = 2
    }

    public enum eJoinType
    {
        Digital = 1,
        Analog = 2,
        Serial = 4
    }

    public class JoinMetadata
    {
        public string Label { get; set; }
        public eJoinType JoinType { get; set; }
        public uint JoinNumber { get; set; }
        public uint JoinSpan { get; set; }
        public eJoinCapabilities JoinCapabilities { get; set; }

    }




}