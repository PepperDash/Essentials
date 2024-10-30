﻿

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

using Newtonsoft.Json;
using Serilog.Events;

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
                    Debug.LogMessage(LogEventLevel.Verbose, "No Join Map found in config with key: '{0}'", joinMapKey);
                    return null;
                }

                Debug.LogMessage(LogEventLevel.Verbose, "Attempting to load custom join map with key: {0}", joinMapKey);

                var joinMapJToken = ConfigReader.ConfigObject.JoinMaps[joinMapKey];

                if (joinMapJToken == null)
                    return null;

                var joinMapData = joinMapJToken.ToObject<Dictionary<string, JoinData>>();

                return joinMapData;
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Error getting join map for key: '{0}'.  Error: {1}", joinMapKey, e);
                return null;
            }
        }

    }

    /// <summary>
    /// Base class for join maps
    /// </summary>
    public abstract class JoinMapBaseAdvanced
    {
        protected uint JoinOffset;

        /// <summary>
        /// The collection of joins and associated metadata
        /// </summary>
        public Dictionary<string, JoinDataComplete> Joins { get; private set; }

        protected JoinMapBaseAdvanced(uint joinStart)
        {
            Joins = new Dictionary<string, JoinDataComplete>();

            JoinOffset = joinStart - 1;
        }

        protected JoinMapBaseAdvanced(uint joinStart, Type type):this(joinStart)
        {
            AddJoins(type);
        }

        protected void AddJoins(Type type)
        {
            type.GetType();
            var fields =
                type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.IsDefined(typeof (JoinNameAttribute), true)).ToList();

            Debug.LogMessage(LogEventLevel.Debug, "Got {fields} with JoinNameAttribute", fields.Count);

            foreach (var field in fields)
            {
                var childClass = Convert.ChangeType(this, type, null);

                //this here is JoinMapBaseAdvanced, not the child class. JoinMapBaseAdvanced has no fields.

                if (!(field.GetValue(childClass) is JoinDataComplete value))
                {
                    Debug.LogMessage(LogEventLevel.Information, "Unable to cast base class to {0}", type.Name);
                    continue;
                }

                value.SetJoinOffset(JoinOffset);

                var joinName = value.GetNameAttribute(field);

                if (string.IsNullOrEmpty(joinName)) continue;

                Joins.Add(joinName, value);
            }


            if (Debug.Level > 0)
            {
                PrintJoinMapInfo();
            }
        }

        /// <summary>
        /// Prints the join information to console
        /// </summary>
        public void PrintJoinMapInfo()
        {
            var sb = JoinmapStringBuilder();

            CrestronConsole.ConsoleCommandResponse(sb.ToString());
        }

        private StringBuilder JoinmapStringBuilder()
        {
            var sb = new StringBuilder();

            var lineEnding = "\r\n";

            var digitals =
                Joins.Where(j => j.Value.Metadata.JoinType.HasFlag(eJoinType.Digital))
                    .ToDictionary(j => j.Key, j => j.Value);

            var analogs = Joins.Where(j => j.Value.Metadata.JoinType.HasFlag(eJoinType.Analog))
                    .ToDictionary(j => j.Key, j => j.Value);

            var serials =
                Joins.Where(j => j.Value.Metadata.JoinType.HasFlag(eJoinType.Serial))
                    .ToDictionary(j => j.Key, j => j.Value);

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Digital join count {digitalCount} Analog join count {analogCount} Serial join count {serialCount}", null, digitals.Count, analogs.Count, serials.Count);

            // Get the joins of each type and print them
            sb.Append($"# {GetType().Name}\r\n");
            sb.Append(lineEnding);
            sb.Append($"## Digitals{lineEnding}");
            sb.Append(lineEnding);
            // Get the joins of each type and print them
            
            var digitalSb = AppendJoinList(GetSortedJoins(digitals));
            digitalSb.Append($"## Analogs{lineEnding}");
            digitalSb.Append(lineEnding);
            
            var analogSb = AppendJoinList(GetSortedJoins(analogs));
            analogSb.Append($"## Serials{lineEnding}");
            analogSb.Append(lineEnding);

            
            var serialSb = AppendJoinList(GetSortedJoins(serials));

            sb.EnsureCapacity(sb.Length + digitalSb.Length + analogSb.Length + serialSb.Length);
            sb.Append(digitalSb).Append(analogSb).Append(serialSb);
            return sb;
        }

        /// <summary>
        /// Prints the join information to console
        /// </summary>
        public void MarkdownJoinMapInfo(string deviceKey, string bridgeKey)
        {
            var pluginType = GetType().Name;

            CrestronConsole.ConsoleCommandResponse("{0}:\n", pluginType);



            WriteJoinmapMarkdown(JoinmapStringBuilder(), pluginType, bridgeKey, deviceKey);

        }

        private static void WriteJoinmapMarkdown(StringBuilder stringBuilder, string pluginType, string bridgeKey, string deviceKey)
        {
            var fileName = string.Format("{0}{1}{2}__{3}__{4}.md", Global.FilePathPrefix, "joinMaps/", pluginType, bridgeKey, deviceKey);

            using (var sw = new StreamWriter(fileName))
            {
                sw.WriteLine(stringBuilder.ToString());
                CrestronConsole.ConsoleCommandResponse("Joinmap Readme generated and written to {0}", fileName);
            }

        }

        /// <summary>
        /// Returns  a sorted list by JoinNumber
        /// </summary>
        /// <param name="joins"></param>
        /// <returns></returns>
        static List<KeyValuePair<string, JoinDataComplete>> GetSortedJoins(Dictionary<string, JoinDataComplete> joins)
        {
            var sortedJoins = joins.ToList();

            sortedJoins.Sort((pair1, pair2) => pair1.Value.JoinNumber.CompareTo(pair2.Value.JoinNumber));

            return sortedJoins;
        }


        static StringBuilder AppendJoinList(List<KeyValuePair<string, JoinDataComplete>> joins)
        {
            var sb = new StringBuilder();
            const string stringFormatter = "| {0} | {1} | {2} | {3} | {4} |\r\n";
            const int joinNumberLen = 11;
            const int joinSpanLen = 9;
            const int typeLen = 19;
            const int capabilitiesLen = 12;
            var descriptionLen = (from @join in joins select @join.Value into j select j.Metadata.Description.Length).Concat(new[] {11}).Max();

            //build header
            sb.Append(string.Format(stringFormatter,
                string.Format("Join Number").PadRight(joinNumberLen, ' '),
                string.Format("Join Span").PadRight(joinSpanLen, ' '),
                string.Format("Description").PadRight(descriptionLen, ' '),
                string.Format("Type").PadRight(typeLen, ' '),
                 string.Format("Capabilities").PadRight(capabilitiesLen, ' ')));
            //build table seperator
            sb.Append(string.Format(stringFormatter,
                new string('-', joinNumberLen),
                new string('-', joinSpanLen),
                new string('-', descriptionLen),
                new string('-', typeLen),
                new string('-', capabilitiesLen)));

            foreach (var join in joins)
            {
                sb.Append(join.Value.GetMarkdownFormattedData(stringFormatter, descriptionLen));
            }
            sb.Append("\r\n");
            return sb;
        }

        /// <summary>
        /// Attempts to find the matching key for the custom join and if found overwrites the default JoinData with the custom
        /// </summary>
        /// <param name="joinData"></param>
        public void SetCustomJoinData(Dictionary<string, JoinData> joinData)
        {
            foreach (var customJoinData in joinData)
            {
                JoinDataComplete join;

                if (!Joins.TryGetValue(customJoinData.Key, out join))
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "No matching key found in join map for: '{0}'", customJoinData.Key);
                    continue;
                }

                if (join != null)
                {
                    join.SetCustomJoinData(customJoinData.Value);
                }
            }

            PrintJoinMapInfo();
        }

        ///// <summary>
        ///// Returns the join number for the join with the specified key
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public uint GetJoinForKey(string key)
        //{
        //    return Joins.ContainsKey(key) ? Joins[key].JoinNumber : 0;
        //}


        ///// <summary>
        ///// Returns the join span for the join with the specified key
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public uint GetJoinSpanForKey(string key)
        //{
        //    return Joins.ContainsKey(key) ? Joins[key].JoinSpan : 0;
        //}
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
        ToFromSIMPL = ToSIMPL | FromSIMPL,
        ToFusion = 4,
        FromFusion = 8,
        ToFromFusion = ToFusion | FromFusion,
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
        DigitalAnalogSerial = Digital | Analog | Serial,
    }

    /// <summary>
    /// Metadata describing the join
    /// </summary>
    public class JoinMetadata
    {
        private string _description;
        /// <summary>
        /// A description for the join to better describe its usage
        /// </summary>
        [JsonProperty("description")]
        public string Description { get { return _description; } set { _description = value; } }
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
    /// Data describing the join.  Can be overridden from configuratino
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
        /// <summary>
        /// Fusion Attribute Name (optional)
        /// </summary>
        [JsonProperty("attributeName")]
        public string AttributeName { get; set; }
    }

    /// <summary>
    /// A class to aggregate the JoinData and JoinMetadata for a join
    /// </summary>
    public class JoinDataComplete
    {
        private uint _joinOffset;

        private JoinData _data;
        public JoinMetadata Metadata { get; set; }
        /// <summary>
        /// To store some future information as you please
        /// </summary>
        public object UserObject { get; private set; }

        public JoinDataComplete(JoinData data, JoinMetadata metadata)
        {
            _data = data;
            Metadata = metadata;
        }

        public string GetMarkdownFormattedData(string stringFormatter, int descriptionLen)
        {

            //Fixed Width Headers
            var joinNumberLen = string.Format("Join Number").Length;
            var joinSpanLen = string.Format("Join Span").Length;
            var typeLen = string.Format("AnalogDigitalSerial").Length;
            var capabilitiesLen = string.Format("ToFromFusion").Length;

            //Track which one failed, if it did
            const string placeholder = "unknown";
            var dataArray = new Dictionary<string, string>
            {
                {"joinNumber", placeholder.PadRight(joinNumberLen, ' ')},
                {"joinSpan", placeholder.PadRight(joinSpanLen, ' ')},
                {"description", placeholder.PadRight(descriptionLen, ' ')},
                {"joinType", placeholder.PadRight(typeLen, ' ')},
                {"capabilities", placeholder.PadRight(capabilitiesLen, ' ')}
            };


            try
            {
                dataArray["joinNumber"] = string.Format("{0}", JoinNumber.ToString(CultureInfo.InvariantCulture).ReplaceIfNullOrEmpty(placeholder)).PadRight(joinNumberLen, ' ');
                dataArray["joinSpan"] = string.Format("{0}", JoinSpan.ToString(CultureInfo.InvariantCulture).ReplaceIfNullOrEmpty(placeholder)).PadRight(joinSpanLen, ' ');
                dataArray["description"] = string.Format("{0}", Metadata.Description.ReplaceIfNullOrEmpty(placeholder)).PadRight(descriptionLen, ' ');
                dataArray["joinType"] = string.Format("{0}", Metadata.JoinType.ToString().ReplaceIfNullOrEmpty(placeholder)).PadRight(typeLen, ' ');
                dataArray["capabilities"] = string.Format("{0}", Metadata.JoinCapabilities.ToString().ReplaceIfNullOrEmpty(placeholder)).PadRight(capabilitiesLen, ' ');

                return string.Format(stringFormatter,
                    dataArray["joinNumber"],
                    dataArray["joinSpan"],
                    dataArray["description"],
                    dataArray["joinType"],
                    dataArray["capabilities"]);

            }
            catch (Exception e)
            {
                //Don't Throw - we don't want to kill the system if this falls over - it's not mission critical. Print the error, use placeholder data
                var errorKey = string.Empty;
                foreach (var item in dataArray)
                {
                    if (item.Value.TrimEnd() == placeholder) continue;
                    errorKey = item.Key;
                    break;
                }
                Debug.LogMessage(LogEventLevel.Information, "Unable to decode join metadata {1}- {0}", e.Message, !string.IsNullOrEmpty(errorKey) ? (' ' + errorKey) : string.Empty);
                return string.Format(stringFormatter,
                    dataArray["joinNumber"],
                    dataArray["joinSpan"],
                    dataArray["description"],
                    dataArray["joinType"],
                    dataArray["capabilities"]);
            }
        }


        /// <summary>
        /// Sets the join offset value
        /// </summary>
        /// <param name="joinOffset"></param>
        public void SetJoinOffset(uint joinOffset)
        {
            _joinOffset = joinOffset;
        }

        /// <summary>
        /// The join number (including the offset)
        /// </summary>
        public uint JoinNumber
        {
            get { return _data.JoinNumber+ _joinOffset; }
            set { _data.JoinNumber = value; }
        }

        public uint JoinSpan
        {
            get { return _data.JoinSpan; }
        }

        public string AttributeName
        {
            get { return _data.AttributeName; }
        }

        public void SetCustomJoinData(JoinData customJoinData)
        {
            _data = customJoinData;
        }

        public string GetNameAttribute(MemberInfo memberInfo)
        {
            var name = string.Empty;
            var attribute = (JoinNameAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(JoinNameAttribute));

            if (attribute == null) return name;

            name = attribute.Name;
            Debug.LogMessage(LogEventLevel.Verbose, "JoinName Attribute value: {0}", name);
            return name;
        }
    }

    

    [AttributeUsage(AttributeTargets.All)]
    public class JoinNameAttribute : Attribute
    {
        private string _Name;

        public JoinNameAttribute(string name)
        {
            Debug.LogMessage(LogEventLevel.Verbose, "Setting Attribute Name: {0}",null, name);
            _Name = name;
        }

        public string Name
        {
            get { return _Name; }
        }
    }
}