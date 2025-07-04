using System;

namespace PepperDash.Essentials.Core.Queues;

public interface IQueueMessage
{
    void Dispatch();
}