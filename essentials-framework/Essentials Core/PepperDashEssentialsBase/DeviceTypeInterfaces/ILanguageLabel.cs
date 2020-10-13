using PepperDash.Core;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    public interface ILanguageLabel:IKeyed
    {
        string Description { get; set; } 
        string DisplayText { get; set; }
    }
}