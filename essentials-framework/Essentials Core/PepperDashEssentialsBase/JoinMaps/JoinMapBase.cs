using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

using Newtonsoft.Json;

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

    /// <summary>
    /// Base class for join maps
    /// </summary>
    [Obsolete("This is being deprecated in favor of JoinMapBaseAdvanced")]
    public abstract class JoinMapBase
    {
        /// <summary>
        /// Modifies all the join numbers by adding the offset.  This should never be called twice
        /// </summary>
        /// <param name="joinStart"></param>
        public abstract void OffsetJoinNumbers(uint joinStart);

        /// <summary>
        /// The collection of joins and associated metadata
        /// </summary>
        public Dictionary<string, JoinMetadata> Joins = new Dictionary<string, JoinMetadata>();

        /// <summary>
        /// Prints the join information to console
        /// </summary>
        public void PrintJoinMapInfo()
        {
            Debug.Console(0, "{0}:\n", this.GetType().Name);

            // Get the joins of each type and print them
            Debug.Console(0, "Digitals:");
            var digitals = Joins.Where(j => (j.Value.JoinType & eJoinType.Digital) == eJoinType.Digital).ToDictionary(j => j.Key, j => j.Value);
            PrintJoinList(GetSortedJoins(digitals));

            Debug.Console(0, "Analogs:");
            var analogs = Joins.Where(j => (j.Value.JoinType & eJoinType.Analog) == eJoinType.Analog).ToDictionary(j => j.Key, j => j.Value);
            PrintJoinList(GetSortedJoins(analogs));

            Debug.Console(0, "Serials:");
            var serials = Joins.Where(j => (j.Value.JoinType & eJoinType.Serial) == eJoinType.Serial).ToDictionary(j => j.Key, j => j.Value);
            PrintJoinList(GetSortedJoins(serials));

        }

        /// <summary>
        /// Returns  a sorted list by JoinNumber
        /// </summary>
        /// <param name="joins"></param>
        /// <returns></returns>
        List<KeyValuePair<string, JoinMetadata>> GetSortedJoins(Dictionary<string, JoinMetadata> joins)
        {
            var sortedJoins = joins.ToList();

            sortedJoins.Sort((pair1, pair2) => pair1.Value.JoinNumber.CompareTo(pair2.Value.JoinNumber));

            return sortedJoins;
        }

        void PrintJoinList(List<KeyValuePair<string, JoinMetadata>> joins)
        {
            foreach (var join in joins)
            {
                Debug.Console(0,
                    @"Join Number: {0} | Label: '{1}' | JoinSpan: '{2}' | Type: '{3}' | Capabilities: '{4}'",
                        join.Value.JoinNumber,
                        join.Value.Label,
                        join.Value.JoinSpan,
                        join.Value.JoinType.ToString(),
                        join.Value.JoinCapabilities.ToString());
            }
        }

        /// <summary>
        /// Returns the join number for the join with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public uint GetJoinForKey(string key)
        {
            if (Joins.ContainsKey(key))
                return Joins[key].JoinNumber;

            else return 0;
        }

        /// <summary>
        /// Returns the join span for the join with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public uint GetJoinSpanForKey(string key)
        {
            if (Joins.ContainsKey(key))
                return Joins[key].JoinSpan;

            else return 0;
        }

    }

    /// <summary>
    /// Base class for join maps
    /// </summary>
    public abstract class JoinMapBaseAdvanced
    {
        /// <summary>
        /// Modifies all the join numbers by adding the offset.  This should never be called twice
        /// </summary>
        /// <param name="joinStart"></param>
        public abstract void OffsetJoinNumbers(uint joinStart);

        /// <summary>
        /// The collection of joins and associated metadata
        /// </summary>
        public Dictionary<string, JoinDataComplete> Joins = new Dictionary<string, JoinDataComplete>();

        /// <summary>
        /// Prints the join information to console
        /// </summary>
        public void PrintJoinMapInfo()
        {
            Debug.Console(0, "{0}:\n", this.GetType().Name);

            // Get the joins of each type and print them
            Debug.Console(0, "Digitals:");
            var digitals = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Digital) == eJoinType.Digital).ToDictionary(j => j.Key, j => j.Value);
            PrintJoinList(GetSortedJoins(digitals));

            Debug.Console(0, "Analogs:");
            var analogs = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Analog) == eJoinType.Analog).ToDictionary(j => j.Key, j => j.Value);
            PrintJoinList(GetSortedJoins(analogs));
            
            Debug.Console(0, "Serials:");
            var serials = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Serial) == eJoinType.Serial).ToDictionary(j => j.Key, j => j.Value);
            PrintJoinList(GetSortedJoins(serials));

        }

        /// <summary>
        /// Returns  a sorted list by JoinNumber
        /// </summary>
        /// <param name="joins"></param>
        /// <returns></returns>
        List<KeyValuePair<string, JoinDataComplete>> GetSortedJoins(Dictionary<string, JoinDataComplete> joins)
        {
            var sortedJoins = joins.ToList();

            sortedJoins.Sort((pair1, pair2) => pair1.Value.JoinNumber.CompareTo(pair2.Value.JoinNumber));

            return sortedJoins;
        }

        void PrintJoinList(List<KeyValuePair<string, JoinDataComplete>> joins)
        {
            foreach (var join in joins)
            {
                Debug.Console(0,
                    @"Join Number: {0} | JoinSpan: '{1}' | Label: '{2}' | Type: '{3}' | Capabilities: '{4}'",
                        join.Value.JoinNumber,
                        join.Value.JoinSpan,
                        join.Value.Metadata.Label,
                        join.Value.Metadata.JoinType.ToString(),
                        join.Value.Metadata.JoinCapabilities.ToString());
            }
        }

        /// <summary>
        /// Returns the join number for the join with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public uint GetJoinForKey(string key)
        {
            return Joins.ContainsKey(key) ? Joins[key].JoinNumber : 0;
        }


        /// <summary>
        /// Returns the join span for the join with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public uint GetJoinSpanForKey(string key)
        {
            return Joins.ContainsKey(key) ? Joins[key].JoinSpan : 0;
        }
    }

    /// <summary>
    /// Read = Provides feedback to SIMPL
    /// Write = Responds to sig values from SIMPL
    /// </summary>
    [Flags]
    public enum eJoinCapabilities
    {
        None = 0,
        ToSIMPL = 1,
        FromSIMPL = 2,
        ToFromSIMPL = ToSIMPL | FromSIMPL
    }

    [Flags]
    public enum eJoinType
    {
        None = 0,
        Digital = 1,
        Analog = 2,
        Serial = 4,
        DigitalAnalog = Digital | Analog,
        DigitalSerial = Digital | Serial,
        AnalogSerial = Analog | Serial,
        DigitalAnalogSerial = Digital | Analog | Serial
    }

    /// <summary>
    /// Metadata describing the join
    /// </summary>
    public class JoinMetadata
    {
        /// <summary>
        /// Join number (based on join offset value)
        /// </summary>
        [JsonProperty("joinNumber")]
        [Obsolete]
        public uint JoinNumber { get; set; }
        /// <summary>
        /// Join range span.  If join indicates the start of a range of joins, this indicated the maximum number of joins in the range
        /// </summary>
        [Obsolete]
        [JsonProperty("joinSpan")]
        public uint JoinSpan { get; set; }

        /// <summary>
        /// A label for the join to better describe it's usage
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }
        /// <summary>
        /// Signal type(s)
        /// </summary>
        [JsonProperty("joinType")]
        public eJoinType JoinType { get; set; }
        /// <summary>
        /// Indicates whether the join is read and/or write
        /// </summary>
        [JsonProperty("joinCapabilities")]
        public eJoinCapabilities JoinCapabilities { get; set; }
        /// <summary>
        /// Indicates a set of valid values (particularly if this translates to an enum
        /// </summary>
        [JsonProperty("validValues")]
        public string[] ValidValues { get; set; }

    }

    /// <summary>
    /// Data describing the join.  Can be 
    /// </summary>
    public class JoinData
    {
        /// <summary>
        /// Join number (based on join offset value)
        /// </summary>
        [JsonProperty("joinNumber")]
        public uint JoinNumber { get; set; }
        /// <summary>
        /// Join range span.  If join indicates the start of a range of joins, this indicated the maximum number of joins in the range
        /// </summary>
        [JsonProperty("joinSpan")]
        public uint JoinSpan { get; set; }
    }

    /// <summary>
    /// A class to aggregate the JoinData and JoinMetadata for a join
    /// </summary>
    public class JoinDataComplete
    {
        private readonly JoinData _data;
        public JoinMetadata Metadata { get; set; }

        public JoinDataComplete(JoinData data, JoinMetadata metadata)
        {
            _data = data;
            Metadata = metadata;
        }

        public uint JoinNumber
        {
            get { return _data.JoinNumber; }
            set { _data.JoinNumber = value; }
        }

        public uint JoinSpan
        {
            get { return _data.JoinSpan; }
        }

        public string GetNameAttribute()
        {
            string name = string.Empty;
            JoinNameAttribute attribute = (JoinNameAttribute)Attribute.GetCustomAttribute(typeof(JoinDataComplete), typeof(JoinNameAttribute));
            if (attribute != null)
            {
                name = attribute.Name;
            }
            return name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class JoinNameAttribute : Attribute
    {
        public string Name { get; set; }

        public JoinNameAttribute(string name)
        {
            Name = name;
        }
    }
}