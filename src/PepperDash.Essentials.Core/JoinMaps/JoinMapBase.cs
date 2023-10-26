extern alias Full;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
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
            Debug.Console(0, "{0}:\n", GetType().Name);

            // Get the joins of each type and print them
            Debug.Console(0, "Digitals:");
            var digitals = Joins.Where(j => (j.Value.JoinType & eJoinType.Digital) == eJoinType.Digital).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Digital Joins", digitals.Count); 
            PrintJoinList(GetSortedJoins(digitals));

            Debug.Console(0, "Analogs:");
            var analogs = Joins.Where(j => (j.Value.JoinType & eJoinType.Analog) == eJoinType.Analog).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Analog Joins", analogs.Count); 
            PrintJoinList(GetSortedJoins(analogs));

            Debug.Console(0, "Serials:");
            var serials = Joins.Where(j => (j.Value.JoinType & eJoinType.Serial) == eJoinType.Serial).ToDictionary(j => j.Key, j => j.Value);
            Debug.Console(2, "Found {0} Serial Joins", serials.Count);
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
}