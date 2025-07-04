using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core;

public interface IProcessorExtensionDeviceFactory
{
    /// <summary>
    /// Loads all the extension factories to the ProcessorExtensionDeviceFactory
    /// </summary>
    void LoadFactories();
}
