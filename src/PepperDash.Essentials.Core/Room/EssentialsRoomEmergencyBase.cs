using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public abstract class EssentialsRoomEmergencyBase : IKeyed
    {
        public string Key { get; private set; }

        public EssentialsRoomEmergencyBase(string key)
        {
            Key = key;
        }
    }
}