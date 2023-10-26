using System;
using System.Collections.Generic;
using System.Globalization;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
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
            _data    = data;
            Metadata = metadata;
        }

        public string GetMarkdownFormattedData(string stringFormatter, int descriptionLen)
        {

            //Fixed Width Headers
            var joinNumberLen   = String.Format("Join Number").Length;
            var joinSpanLen     = String.Format("Join Span").Length;
            var typeLen         = String.Format("AnalogDigitalSerial").Length;
            var capabilitiesLen = String.Format("ToFromFusion").Length;

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
                dataArray["joinNumber"]   = String.Format("{0}", JoinNumber.ToString(CultureInfo.InvariantCulture).ReplaceIfNullOrEmpty(placeholder)).PadRight(joinNumberLen, ' ');
                dataArray["joinSpan"]     = String.Format("{0}", JoinSpan.ToString(CultureInfo.InvariantCulture).ReplaceIfNullOrEmpty(placeholder)).PadRight(joinSpanLen, ' ');
                dataArray["description"]  = String.Format("{0}", Metadata.Description.ReplaceIfNullOrEmpty(placeholder)).PadRight(descriptionLen, ' ');
                dataArray["joinType"]     = String.Format("{0}", Metadata.JoinType.ToString().ReplaceIfNullOrEmpty(placeholder)).PadRight(typeLen, ' ');
                dataArray["capabilities"] = String.Format("{0}", Metadata.JoinCapabilities.ToString().ReplaceIfNullOrEmpty(placeholder)).PadRight(capabilitiesLen, ' ');

                return String.Format(stringFormatter,
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
                Debug.Console(0, "Unable to decode join metadata {1}- {0}", e.Message, !String.IsNullOrEmpty(errorKey) ? (' ' + errorKey) : String.Empty);
                return String.Format(stringFormatter,
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
            var name      = string.Empty;
            var attribute = (JoinNameAttribute)CAttribute.GetCustomAttribute(memberInfo, typeof(JoinNameAttribute));

            if (attribute == null) return name;

            name = attribute.Name;
            Debug.Console(2, "JoinName Attribute value: {0}", name);
            return name;
        }
    }
}