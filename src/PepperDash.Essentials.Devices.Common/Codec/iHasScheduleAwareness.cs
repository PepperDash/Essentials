extern alias Full;
using System.Linq;
using System.Text;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasScheduleAwareness
    {
        CodecScheduleAwareness CodecSchedule { get; }

        void GetSchedule();
    }
}
