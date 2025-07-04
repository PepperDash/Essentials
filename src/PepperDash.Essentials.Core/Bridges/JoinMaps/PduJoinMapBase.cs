using System;

namespace PepperDash.Essentials.Core.Bridges;

public class PduJoinMapBase : JoinMapBaseAdvanced
{
    [JoinName("Name")]
    public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
        new JoinMetadata { Description = "PDU Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

    [JoinName("Online")]
    public JoinDataComplete Online = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
        new JoinMetadata { Description = "Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

    [JoinName("OutletCount")]
    public JoinDataComplete OutletCount = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
        new JoinMetadata { Description = "Number of COntrolled Outlets", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

    [JoinName("OutletName")]
    public JoinDataComplete OutletName = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
        new JoinMetadata { Description = "Outlet Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

    [JoinName("OutletEnabled")]
    public JoinDataComplete OutletEnabled = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
        new JoinMetadata { Description = "Outlet Enabled", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

    [JoinName("OutletPowerCycle")]
    public JoinDataComplete OutletPowerCycle = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
        new JoinMetadata { Description = "Outlet Power Cycle", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

    [JoinName("OutletPowerOn")]
    public JoinDataComplete OutletPowerOn = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
        new JoinMetadata { Description = "Outlet Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
    
    [JoinName("OutletPowerOff")]
    public JoinDataComplete OutletPowerOff = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
        new JoinMetadata { Description = "Outlet Power Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
    


    /// <summary>
    /// Constructor to use when instantiating this Join Map without inheriting from it
    /// </summary>
    /// <param name="joinStart">Join this join map will start at</param>
    public PduJoinMapBase(uint joinStart)
        :base(joinStart, typeof(PduJoinMapBase))
    {   
    }

    /// <summary>
    /// Constructor to use when extending this Join map
    /// </summary>
    /// <param name="joinStart">Join this join map will start at</param>
    /// <param name="type">Type of the child join map</param>
    public PduJoinMapBase(uint joinStart, Type type)
        : base(joinStart, type)
    {            
    }
}