using System;

namespace PepperDash.Essentials.Core.Queues
{
    public interface IQueueMessage
    {
        void Dispatch();
    }
}

namespace PepperDash_Essentials_Core.Queues
{
    [Obsolete("Use PepperDash.Essentials.Core.Queues")]
    public interface IQueueMessage:PepperDash.Essentials.Core.Queues.IQueueMessage
    {
    }
}