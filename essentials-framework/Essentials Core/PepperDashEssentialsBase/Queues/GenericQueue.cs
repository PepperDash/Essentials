using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Queues
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

        private bool _delayEnabled;
        private int _delayTime;

        private const Thread.eThreadPriority _defaultPriority = Thread.eThreadPriority.MediumPriority;

        /// <summary>
        /// If the instance has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Returns the capacity of the CrestronQueue (fixed Size property)
        /// </summary>
        public int QueueCapacity
        {
            get
            {
                return _queue.Size;
            }
        }

        /// <summary>
        /// Returns the number of elements currently in the CrestronQueue
        /// </summary>    
        public int QueueCount
        {
            get
            {
                return _queue.Count;
            }
        }

        /// <summary>
        /// Constructor with no thread priority
        /// </summary>
        /// <param name="key"></param>
        public GenericQueue(string key)
            : this(key, _defaultPriority, 0, 0)
        {
        }

        /// <summary>
        /// Constructor with queue size
        /// </summary>
        /// <param name="key"></param>
        /// <param name="capacity">Fixed size for the queue to hold</param>
        public GenericQueue(string key, int capacity)
            : this(key, _defaultPriority, capacity, 0)
        {
        }

        /// <summary>
        /// Constructor for generic queue with no pacing
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="pacing">Pacing in ms between actions</param>
        public GenericQueue(int pacing, string key)
            : this(key, _defaultPriority, 0, pacing)
        {
        }

        /// <summary>
        /// Constructor with pacing and capacity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pacing"></param>
        /// <param name="capacity"></param>
        public GenericQueue(string key, int pacing, int capacity)
            : this(key, _defaultPriority, capacity, pacing)
        {
        }

        /// <summary>
        /// Constructor with pacing and priority
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pacing"></param>
        /// <param name="priority"></param>
        public GenericQueue(string key, int pacing, Thread.eThreadPriority priority)
            : this(key, priority, 0, pacing)
        {
        }

        /// <summary>
        /// Constructor with pacing, priority and capacity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="priority"></param>
        /// <param name="capacity"></param>
        public GenericQueue(string key, Thread.eThreadPriority priority, int capacity)
            : this(key, priority, capacity, 0)
        {
        }

        /// <summary>
        /// Constructor with pacing, priority and capacity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pacing"></param>
        /// <param name="priority"></param>
        /// <param name="capacity"></param>
        public GenericQueue(string key, int pacing, Thread.eThreadPriority priority, int capacity)
            : this(key, priority, capacity, pacing)
        {
        }

        /// <summary>
        /// Constructor for generic queue with no pacing
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="priority"></param>
        /// <param name="capacity"></param>
        /// <param name="pacing"></param>
        protected GenericQueue(string key, Thread.eThreadPriority priority, int capacity, int pacing)
        {
            _key = key;
            int cap = 25; // sets default
            if (capacity > 0)
            {
                cap = capacity; // overrides default
            }
            _queue = new CrestronQueue<IQueueMessage>(cap);
            _worker = new Thread(ProcessQueue, null, Thread.eThreadStartOptions.Running)
            {
                Priority = priority,
                Name = _key
            };

            SetDelayValues(pacing);
        }

        private void SetDelayValues(int pacing)
        {
            _delayEnabled = pacing > 0;
            _delayTime = pacing;

            CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
            {
                if (programEvent != eProgramStatusEventType.Stopping)
                    return;

                Dispose(true);
            };
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
                        //Debug.Console(2, this, "Processing queue item: '{0}'", item.ToString());
                        item.Dispatch();

                        if (_delayEnabled)
                            Thread.Sleep(_delayTime);
                    }
                    catch (Exception ex)
                    {
                        Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Caught an exception in the Queue {0}\r{1}\r{2}", ex.Message, ex.InnerException, ex.StackTrace);
                    }
                }
                else _waitHandle.Wait();
            }

            return null;
        }

        public void Enqueue(IQueueMessage item)
        {
            if (Disposed)
            {
                Debug.Console(1, this, "I've been disposed so you can't enqueue any messages.  Are you trying to dispatch a message while the program is stopping?");
                return;
            }

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
                Debug.Console(2, this, "Disposing...");
                if (_queue != null && !_queue.Disposed)
                {
                    _queue.Clear();
                    Enqueue(null);
                }
                _worker.Abort();
                _waitHandle.Close();
            }

            Disposed = true;
        }

        ~GenericQueue()
        {
            Dispose(true);
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

namespace PepperDash_Essentials_Core.Queues
{
    /// <summary>
    /// Threadsafe processing of queued items with pacing if required
    /// </summary>
    [Obsolete("Use PepperDash.Essentials.Core.Queues")]
    public class GenericQueue : IQueue<IQueueMessage>
    {
        private readonly string _key;
        protected readonly CrestronQueue<IQueueMessage> _queue;
        protected readonly Thread _worker;
        protected readonly CEvent _waitHandle = new CEvent();

        private bool _delayEnabled;
        private int _delayTime;

        private const Thread.eThreadPriority _defaultPriority = Thread.eThreadPriority.MediumPriority;

        /// <summary>
        /// If the instance has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Returns the capacity of the CrestronQueue (fixed Size property)
        /// </summary>
        public int QueueCapacity
        {
            get
            {
                return _queue.Size;
            }
        }

        /// <summary>
        /// Returns the number of elements currently in the CrestronQueue
        /// </summary>    
        public int QueueCount
        {
            get
            {
                return _queue.Count;
            }
        }

        /// <summary>
        /// Constructor with no thread priority
        /// </summary>
        /// <param name="key"></param>
        public GenericQueue(string key)
            : this(key, _defaultPriority, 0, 0)
        {
        }

        /// <summary>
        /// Constructor with queue size
        /// </summary>
        /// <param name="key"></param>
        /// <param name="capacity">Fixed size for the queue to hold</param>
        public GenericQueue(string key, int capacity)
            : this(key, _defaultPriority, capacity, 0)
        {
        }

        /// <summary>
        /// Constructor for generic queue with no pacing
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="pacing">Pacing in ms between actions</param>
        public GenericQueue(int pacing, string key)
            : this(key, _defaultPriority, 0, pacing)
        {
        }

        /// <summary>
        /// Constructor with pacing and capacity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pacing"></param>
        /// <param name="capacity"></param>
        public GenericQueue(string key, int pacing, int capacity)
            : this(key, _defaultPriority, capacity, pacing)
        {
        }

        /// <summary>
        /// Constructor with pacing and priority
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pacing"></param>
        /// <param name="priority"></param>
        public GenericQueue(string key, int pacing, Thread.eThreadPriority priority)
            : this(key, priority, 0, pacing)
        {
        }

        /// <summary>
        /// Constructor with pacing, priority and capacity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="priority"></param>
        /// <param name="capacity"></param>
        public GenericQueue(string key, Thread.eThreadPriority priority, int capacity)
            : this(key, priority, capacity, 0)
        {
        }

        /// <summary>
        /// Constructor with pacing, priority and capacity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pacing"></param>
        /// <param name="priority"></param>
        /// <param name="capacity"></param>
        public GenericQueue(string key, int pacing, Thread.eThreadPriority priority, int capacity)
            : this(key, priority, capacity, pacing)
        {
        }

        /// <summary>
        /// Constructor for generic queue with no pacing
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="priority"></param>
        /// <param name="capacity"></param>
        /// <param name="pacing"></param>
        protected GenericQueue(string key, Thread.eThreadPriority priority, int capacity, int pacing)
        {
            _key = key;
            int cap = 25; // sets default
            if (capacity > 0)
            {
                cap = capacity; // overrides default
            }
            _queue = new CrestronQueue<IQueueMessage>(cap);
            _worker = new Thread(ProcessQueue, null, Thread.eThreadStartOptions.Running)
            {
                Priority = priority
            };

            SetDelayValues(pacing);
        }

        private void SetDelayValues(int pacing)
        {
            _delayEnabled = pacing > 0;
            _delayTime = pacing;

            CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
            {
                if (programEvent != eProgramStatusEventType.Stopping)
                    return;

                Dispose(true);
            };
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
                        Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Caught an exception in the Queue {0}\r{1}\r{2}", ex.Message, ex.InnerException, ex.StackTrace);
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
                Debug.Console(2, this, "Disposing...");
                if (_queue != null && !_queue.Disposed)
                {
                    _queue.Clear();
                    Enqueue(null);
                }
                _worker.Abort();
                _waitHandle.Close();
            }

            Disposed = true;
        }

        ~GenericQueue()
        {
            Dispose(true);
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