using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

    /// <summary>
    /// Describes a device that has selectable inputs
    /// </summary>
    public interface IHasInputs
    {
        ISelectableItems<string> Inputs { get; }
    }
}
