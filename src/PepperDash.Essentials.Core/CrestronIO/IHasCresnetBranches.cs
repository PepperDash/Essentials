using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Defines the contract for IHasCresnetBranches
    /// </summary>
    public interface IHasCresnetBranches
    {
        CrestronCollection<CresnetBranch> CresnetBranches { get; }
    }
}