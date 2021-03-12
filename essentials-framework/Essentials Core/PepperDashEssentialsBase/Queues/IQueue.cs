using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Queues
{
    public interface IQueue<T> : IKeyed, IDisposable where T : class 
    {
        void Enqueue(T item);
        bool Disposed { get; }
    }
}

namespace PepperDash_Essentials_Core.Queues
{
    [Obsolete("Use PepperDash.Essentials.Core.Queues")]
    public interface IQueue<T> : IKeyed, IDisposable where T : class
    {
        void Enqueue(T item);
        bool Disposed { get; }
    }
}