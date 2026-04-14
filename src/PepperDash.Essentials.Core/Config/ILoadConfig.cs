using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines the contract for loading configuration.  This is called after all plugins have been loaded, but before any devices are initialized.  This allows for any necessary configuration merging or processing to be done before devices are created and initialized.
/// </summary>
public interface ILoadConfig
{
    /// <summary>
    /// Executes the loading of the configuration.  This is called after all plugins have been loaded, but before any devices are initialized.  This allows for any necessary configuration merging or processing to be done before devices are created and initialized.
    /// </summary>
    void GoWithLoad();
}
