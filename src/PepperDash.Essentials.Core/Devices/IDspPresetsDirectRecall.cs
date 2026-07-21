using PepperDash.Core;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core;


/// <summary>
/// Defines the contract for IDspPresetsDirectRecall
/// </summary>
public interface IDspPresetsDirectRecall : IKeyed
{

    /// <summary>
    /// Recalls the preset by name
    /// </summary>
    /// <param name="name"></param>
    void RunPreset(string name);
}