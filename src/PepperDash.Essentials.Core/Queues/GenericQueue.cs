using System;
using System.Collections.Concurrent;
using System.Threading;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Queues;


// TODO: The capacity argument in the constructors should be removed.  Now that this class uses System.Threading rather than the Crestron library, there is no longer a thread capacity limit.  
// If a capacity limit is needed, it should be implemented by the caller by checking the QueueCount property before enqueuing items and deciding how to handle the situation when the queue is too full (e.g. drop messages, log warnings, etc.)

/// <summary>
/// Threadsafe processing of queued items with pacing if required
/// </summary>
public class GenericQueue : IQueue<IQueueMessage>
{
    private readonly string _key;

    /// <summary>
    /// Returns the number of items currently in the queue.  This is not threadsafe, so it should only be used for informational purposes and not for processing logic.
    /// </summary>
    protected readonly ConcurrentQueue<IQueueMessage> _queue;
    
    /// <summary>
    /// The thread that processes the queue items
    /// </summary>
    protected readonly Thread _worker;
    private readonly object _lock = new();

    private bool _delayEnabled;
    private int _delayTime;

    private const ThreadPriority _defaultPriority = ThreadPriority.Normal;

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
            return int.MaxValue;
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
    public GenericQueue(string key, int pacing, ThreadPriority priority)
        : this(key, priority, 0, pacing)
    {
    }

    /// <summary>
    /// Constructor with pacing, priority and capacity
    /// </summary>
    /// <param name="key"></param>
    /// <param name="priority"></param>
    /// <param name="capacity"></param>
    public GenericQueue(string key, ThreadPriority priority, int capacity)
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
    public GenericQueue(string key, int pacing, ThreadPriority priority, int capacity)
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
    protected GenericQueue(string key, ThreadPriority priority, int capacity, int pacing)
    {
        _key = key;

        _queue = new ConcurrentQueue<IQueueMessage>();
        _worker = new Thread(ProcessQueue)
        {
            Priority = priority,
            Name = _key,
            IsBackground = true
        };
        _worker.Start();

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
    /// <returns>Null when the thread is exited</returns>
    private void ProcessQueue()
    {
        while (true)
        {
            if (_queue.TryDequeue(out var item) && item == null)
                break;

            if (item != null)
            {
                try
                {
                    //Debug.LogMessage(LogEventLevel.Verbose, this, "Processing queue item: '{0}'", item.ToString());
                    item.Dispatch();

                    if (_delayEnabled)
                        Thread.Sleep(_delayTime);
                }
                catch (ThreadInterruptedException)
                {
                    //swallowing this exception, as it should only happen on shut down
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(LogEventLevel.Information, this, "Caught an exception in the Queue: {1}:{0}", ex.Message, ex);
                    Debug.LogMessage(LogEventLevel.Verbose, this, "Stack Trace: {0}", ex.StackTrace);

                    if (ex.InnerException != null)
                    {
                        Debug.LogMessage(LogEventLevel.Information, this, "---\r\n{0}", ex.InnerException.Message);
                        Debug.LogMessage(LogEventLevel.Verbose, this, "Stack Trace: {0}", ex.InnerException.StackTrace);
                    }
                }
            }
            else
            {
                lock (_lock)
                {
                    if (_queue.IsEmpty)
                        Monitor.Wait(_lock);
                }
            }
        }
    }

    /// <summary>
    /// Enqueues an item to be processed by the queue thread.  If the queue has been disposed, the item will not be enqueued and a message will be logged.
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(IQueueMessage item)
    {
        if (Disposed)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Queue has been disposed. Enqueuing messages not allowed while program is stopping.");
            return;
        }

        _queue.Enqueue(item);
        lock (_lock)
            Monitor.Pulse(_lock);
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
            Debug.LogMessage(LogEventLevel.Verbose, this, "Disposing...");
            _queue.Enqueue(null);
            lock (_lock)
                Monitor.Pulse(_lock);
            _worker.Join();
        }

        Disposed = true;
    }

    /// Finalizer in case Dispose is not called.  This will clean up the thread, but any items still in the queue will not be processed and could potentially be lost.
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
