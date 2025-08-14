using System;
using System.Collections.Generic;
using System.Threading;

namespace Crestron.SimplSharp
{
  /// <summary>
  /// Mock implementation of Crestron CrestronQueue for testing purposes
  /// Provides the same public API surface as the real CrestronQueue
  /// </summary>
  /// <typeparam name="T">Type of items in the queue</typeparam>
  public class CrestronQueue<T> : IDisposable
  {
    #region Private Fields

    private readonly Queue<T> _queue = new Queue<T>();
    private readonly object _lockObject = new object();
    private readonly ManualResetEventSlim _dataAvailableEvent = new ManualResetEventSlim(false);
    private bool _disposed = false;

    #endregion

    #region Properties

    /// <summary>Gets the number of items in the queue</summary>
    public int Count
    {
      get
      {
        lock (_lockObject)
        {
          return _queue.Count;
        }
      }
    }

    /// <summary>Gets whether the queue is empty</summary>
    public bool IsEmpty
    {
      get
      {
        lock (_lockObject)
        {
          return _queue.Count == 0;
        }
      }
    }

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the CrestronQueue class</summary>
    public CrestronQueue()
    {
      // Mock implementation
    }

    #endregion

    #region Public Methods

    /// <summary>Adds an item to the end of the queue</summary>
    /// <param name="item">Item to add</param>
    public void Enqueue(T item)
    {
      if (_disposed) throw new ObjectDisposedException(nameof(CrestronQueue<T>));

      lock (_lockObject)
      {
        _queue.Enqueue(item);
        _dataAvailableEvent.Set();
      }
    }

    /// <summary>Removes and returns the item at the beginning of the queue</summary>
    /// <returns>The item that was removed from the queue</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty</exception>
    public T Dequeue()
    {
      if (_disposed) throw new ObjectDisposedException(nameof(CrestronQueue<T>));

      lock (_lockObject)
      {
        if (_queue.Count == 0)
          throw new InvalidOperationException("Queue is empty");

        var item = _queue.Dequeue();

        if (_queue.Count == 0)
          _dataAvailableEvent.Reset();

        return item;
      }
    }

    /// <summary>Tries to remove and return the item at the beginning of the queue</summary>
    /// <param name="item">When successful, contains the dequeued item</param>
    /// <returns>True if an item was successfully dequeued</returns>
    public bool TryDequeue(out T item)
    {
      if (_disposed)
      {
        item = default(T)!;
        return false;
      }

      lock (_lockObject)
      {
        if (_queue.Count == 0)
        {
          item = default(T)!;
          return false;
        }

        item = _queue.Dequeue();

        if (_queue.Count == 0)
          _dataAvailableEvent.Reset();

        return true;
      }
    }

    /// <summary>Returns the item at the beginning of the queue without removing it</summary>
    /// <returns>The item at the beginning of the queue</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty</exception>
    public T Peek()
    {
      if (_disposed) throw new ObjectDisposedException(nameof(CrestronQueue<T>));

      lock (_lockObject)
      {
        if (_queue.Count == 0)
          throw new InvalidOperationException("Queue is empty");

        return _queue.Peek();
      }
    }

    /// <summary>Tries to return the item at the beginning of the queue without removing it</summary>
    /// <param name="item">When successful, contains the item at the beginning of the queue</param>
    /// <returns>True if an item was found</returns>
    public bool TryPeek(out T item)
    {
      if (_disposed)
      {
        item = default(T)!;
        return false;
      }

      lock (_lockObject)
      {
        if (_queue.Count == 0)
        {
          item = default(T)!;
          return false;
        }

        item = _queue.Peek();
        return true;
      }
    }

    /// <summary>Removes all items from the queue</summary>
    public void Clear()
    {
      if (_disposed) return;

      lock (_lockObject)
      {
        _queue.Clear();
        _dataAvailableEvent.Reset();
      }
    }

    /// <summary>Waits for data to become available in the queue</summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>True if data became available within the timeout</returns>
    public bool WaitForData(int timeout)
    {
      if (_disposed) return false;

      return _dataAvailableEvent.Wait(timeout);
    }

    /// <summary>Waits for data to become available in the queue</summary>
    /// <returns>True when data becomes available</returns>
    public bool WaitForData()
    {
      if (_disposed) return false;

      _dataAvailableEvent.Wait();
      return true;
    }

    /// <summary>Copies the queue elements to an array</summary>
    /// <returns>Array containing the queue elements</returns>
    public T[] ToArray()
    {
      if (_disposed) return new T[0];

      lock (_lockObject)
      {
        return _queue.ToArray();
      }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>Disposes the queue and releases resources</summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>Protected dispose method</summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          _dataAvailableEvent?.Dispose();
          Clear();
        }

        _disposed = true;
      }
    }

    #endregion
  }
}
