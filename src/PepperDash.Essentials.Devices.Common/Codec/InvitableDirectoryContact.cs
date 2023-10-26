extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class InvitableDirectoryContact : DirectoryContact, IInvitableContact
    {
        [JsonProperty("isInvitableContact")]
        public bool IsInvitableContact
        {
            get
            {
                return this is IInvitableContact;
            }
        }
    }
}