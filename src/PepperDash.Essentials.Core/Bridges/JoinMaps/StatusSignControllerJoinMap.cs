using System;

namespace PepperDash.Essentials.Core.Bridges;

public class StatusSignControllerJoinMap : JoinMapBaseAdvanced
{
    [JoinName("IsOnline")]
    public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Sign Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

    [JoinName("Name")]
    public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Sign Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

    [JoinName("RedControl")]
    public JoinDataComplete RedControl = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Red LED Enable / Disable", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

    [JoinName("RedLed")]
    public JoinDataComplete RedLed = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Red LED Intensity", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

    [JoinName("GreenControl")]
    public JoinDataComplete GreenControl = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Green LED Enable / Disable", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

    [JoinName("GreenLed")]
    public JoinDataComplete GreenLed = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Green LED Intensity", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

    [JoinName("BlueControl")]
    public JoinDataComplete BlueControl = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Blue LED Enable / Disable", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

    [JoinName("BlueLed")]
    public JoinDataComplete BlueLed = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
        new JoinMetadata { Description = "Status Blue LED Intensity", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

    /// <summary>
    /// Constructor to use when instantiating this Join Map without inheriting from it
    /// </summary>
    /// <param name="joinStart">Join this join map will start at</param>
    public StatusSignControllerJoinMap(uint joinStart)
        : this(joinStart, typeof(StatusSignControllerJoinMap))
    {
    }

    /// <summary>
    /// Constructor to use when extending this Join map
    /// </summary>
    /// <param name="joinStart">Join this join map will start at</param>
    /// <param name="type">Type of the child join map</param>
    protected StatusSignControllerJoinMap(uint joinStart, Type type)
        : base(joinStart, type)
    {
    }

}