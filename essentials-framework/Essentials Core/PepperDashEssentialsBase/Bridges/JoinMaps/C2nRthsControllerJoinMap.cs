using System.Linq;
using Crestron.SimplSharp.Reflection;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class C2nRthsControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Temp Sensor Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("TemperatureFormat")]
        public JoinDataComplete TemperatureFormat = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Temp Sensor Unit Format", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Temperature")]
        public JoinDataComplete Temperature = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Temp Sensor Temperature Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Humidity")]
        public JoinDataComplete Humidity = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Temp Sensor Humidity Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Temp Sensor Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        public C2nRthsControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(C2nRthsControllerJoinMap))
        {
        }
    }
}