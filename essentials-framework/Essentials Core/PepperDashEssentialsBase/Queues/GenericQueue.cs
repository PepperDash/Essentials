using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using PepperDash.Core;

namespace PepperDash_Essentials_Core.Queues
{
    /// <summary>
    /// Threadsafe processing of queued items with pacing if required
    /// </summary>
    public class GenericQueue : IQueue<IQueueMessage>
    {
        private readonly string _key;
        protected readonly CrestronQueue<IQueueMessage> _queue;
        protected readonly Thread _worker;
        protected readonly CEvent _waitHandle = new CEvent();
        
        private readonly bool _delayEnabled;
        private readonly int _delayTime;

        /// <summary>
        /// If the instance has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Constructor for generic queue with no pacing
        /// </summary>
        /// <param name="key">Key</param>
        public GenericQueue(string key)
        {
            _key = key;
            _queue = new CrestronQueue<IQueueMessage>();
            _worker = new Thread(ProcessQueue, null, Thread.eThreadStartOptions.Running);

            CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
            {
                if (programEvent != eProgramStatusEventType.Stopping)
                    return;

                Dispose();
            };
        }

        /// <summary>
        /// Constructor for generic queue with no pacing
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="pacing">Pacing in ms between actions</param>
        public GenericQueue(string key, int pacing)
            : this(key)
        {
            _delayEnabled = pacing > 0;
            _delayTime = pacing;
        }

        /// <summary>
        /// Thread callback
        /// </summary>
        /// <param name="obj">The action used to process dequeued items</param>
        /// <returns>Null when the thread is exited</returns>
        private object ProcessQueue(object obj)
        {
            while (true)
            {
                IQueueMessage item = null;

                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                    if (item == null)
                        break;
                }
                if (item != null)
                {
                    try
                    {
                        Debug.Console(2, this, "Processing queue item: '{0}'", item.ToString());
                        item.Dispatch();

                        if (_delayEnabled)
                            Thread.Sleep(_delayTime);
                    }
                    catch (Exception ex)
                    {
                        Debug.ConsoleWithLog(0, this, "Caught an exception in the Queue {0}\r{1}\r{2}", ex.Message, ex.InnerException, ex.StackTrace);
                    }
                }
                else _waitHandle.Wait();
            }

            return null;
        }

        public void Enqueue(IQueueMessage item)
        {
            _queue.Enqueue(item);
            _waitHandle.Set();
        }

        /// <summary>
        /// Disposes the thread and cleans up resources.  Thread cannot be restarted once
        /// disposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Actually does the disposing.  If you override this method, be sure to either call the base implementation 
        /// or clean up all the resources yourself.
        /// </summary>
        /// <param name="disposing">set to true unless called from finalizer</param>
        protected void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                Enqueue(null);
                _worker.Join();
                _waitHandle.Close();
            }

            Disposed = true;
        }

        ~GenericQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// Key
        /// </summary>
        public string Key
        {
            get { return _key; }
        }
    }
}