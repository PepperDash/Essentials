using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
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
            var fields =
                type.GetCType()
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.IsDefined(typeof (JoinNameAttribute), true));

            foreach (var field in fields)
            {
                var childClass = Convert.ChangeType(this, type, null);

                var value = field.GetValue(childClass) as JoinDataComplete; //this here is JoinMapBaseAdvanced, not the child class. JoinMapBaseAdvanced has no fields.

                if (value == null)
                {
                    Debug.Console(0, "Unable to cast base class to {0}", type.Name);
                    continue;
                }

                value.SetJoinOffset(JoinOffset);

                var joinName = value.GetNameAttribute(field);

                if (String.IsNullOrEmpty(joinName)) continue;

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
            Debug.Console(0, "{0}:\n", GetType().Name);

            // Get the joins of each type and print them
            Debug.Console(0, "Digitals:");
            var digitals = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Digital) == eJoinType.Digital).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Digital Joins", digitals.Count);
            PrintJoinList(GetSortedJoins(digitals));

            Debug.Console(0, "Analogs:");
            var analogs = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Analog) == eJoinType.Analog).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Analog Joins", analogs.Count);
            PrintJoinList(GetSortedJoins(analogs));

            Debug.Console(0, "Serials:");
            var serials = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Serial) == eJoinType.Serial).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Serial Joins", serials.Count);
            PrintJoinList(GetSortedJoins(serials));

        }
        /// <summary>
        /// Prints the join information to console
        /// </summary>
        public void MarkdownJoinMapInfo(string deviceKey, string bridgeKey)
        {
            var pluginType = GetType().Name;

            Debug.Console(0, "{0}:\n", pluginType);

            var sb = new StringBuilder();

            sb.AppendLine(String.Format("# {0}", GetType().Name));
            sb.AppendLine(String.Format("Generated from '{0}' on bridge '{1}'", deviceKey, bridgeKey));
            sb.AppendLine();
            sb.AppendLine("## Digitals");
            // Get the joins of each type and print them
            var digitals = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Digital) == eJoinType.Digital).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Digital Joins", digitals.Count);
            var digitalSb = AppendJoinList(GetSortedJoins(digitals));
            digitalSb.AppendLine("## Analogs");
            digitalSb.AppendLine();

            Debug.Console(0, "Analogs:");
            var analogs = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Analog) == eJoinType.Analog).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Analog Joins", analogs.Count);
            var analogSb = AppendJoinList(GetSortedJoins(analogs));
            analogSb.AppendLine("## Serials");
            analogSb.AppendLine();

            Debug.Console(0, "Serials:");
            var serials = Joins.Where(j => (j.Value.Metadata.JoinType & eJoinType.Serial) == eJoinType.Serial).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Serial Joins", serials.Count);
            var serialSb = AppendJoinList(GetSortedJoins(serials));

            sb.EnsureCapacity(sb.Length + digitalSb.Length + analogSb.Length + serialSb.Length);
            sb.Append(digitalSb).Append(analogSb).Append(serialSb);

            WriteJoinmapMarkdown(sb, pluginType, bridgeKey, deviceKey);

        }

        private static void WriteJoinmapMarkdown(StringBuilder stringBuilder, string pluginType, string bridgeKey, string deviceKey)
        {
            var fileName = String.Format("{0}{1}{2}__{3}__{4}.md", Global.FilePathPrefix, "joinMaps/", pluginType, bridgeKey, deviceKey);

            using (var sw = new StreamWriter(fileName))
            {
                sw.WriteLine(stringBuilder.ToString());
                Debug.Console(0, "Joinmap Readme generated and written to {0}", fileName);
            }

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
                    @"Join Number: {0} | JoinSpan: '{1}' | JoinName: {2} | Description: '{3}' | Type: '{4}' | Capabilities: '{5}'",
                    join.Value.JoinNumber,
                    join.Value.JoinSpan,
                    join.Key,
                    String.IsNullOrEmpty(join.Value.AttributeName) ? join.Value.Metadata.Label : join.Value.AttributeName,
                    join.Value.Metadata.JoinType.ToString(),
                    join.Value.Metadata.JoinCapabilities.ToString());
            }
        }

        static StringBuilder AppendJoinList(List<KeyValuePair<string, JoinDataComplete>> joins)
        {
            var          sb              = new StringBuilder();
            const string stringFormatter = "| {0} | {1} | {2} | {3} | {4} |";
            const int    joinNumberLen   = 11;
            const int    joinSpanLen     = 9;
            const int    typeLen         = 19;
            const int    capabilitiesLen = 12;
            var          descriptionLen  = (from @join in joins select @join.Value into j select j.Metadata.Description.Length).Concat(new[] {11}).Max();

            //build header
            sb.AppendLine(String.Format(stringFormatter, 
                String.Format("Join Number").PadRight(joinNumberLen, ' '), 
                String.Format("Join Span").PadRight(joinSpanLen, ' '), 
                String.Format("Description").PadRight(descriptionLen, ' '), 
                String.Format("Type").PadRight(typeLen, ' '),
                String.Format("Capabilities").PadRight(capabilitiesLen, ' ')));
            //build table seperator
            sb.AppendLine(String.Format(stringFormatter,
                new String('-', joinNumberLen),
                new String('-', joinSpanLen),
                new String('-', descriptionLen),
                new String('-', typeLen),
                new String('-', capabilitiesLen)));

            foreach (var join in joins)
            {
                sb.AppendLine(join.Value.GetMarkdownFormattedData(stringFormatter, descriptionLen));
            }
            sb.AppendLine();
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
                var join = Joins[customJoinData.Key];

                if (join != null)
                {
                    join.SetCustomJoinData(customJoinData.Value);
                }
                else
                {
                    Debug.Console(2, "No matching key found in join map for: '{0}'", customJoinData.Key);
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
}