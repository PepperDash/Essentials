using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    public class DgeJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DGE Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });        

        [JoinName("CurrentInputResolution")]
        public JoinDataComplete CurrentInputResolution = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "DGE Current Input Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

		[JoinName("SyncDetected")]
		public JoinDataComplete SyncDetected = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata { Description = "DGE Sync Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HdmiHdcpOn")]
        public JoinDataComplete HdmiInHdcpOn = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "DGE HDMI HDCP State On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HdmiHdcpOff")]
        public JoinDataComplete HdmiInHdcpOff = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "DGE HDMI HDCP State Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HdmiHdcpToggle")]
        public JoinDataComplete HdmiInHdcpToggle = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "DGE HDMI HDCP State Toggle", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        public DgeJoinMap(uint joinStart)
            : this(joinStart, typeof(DgeJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected DgeJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}