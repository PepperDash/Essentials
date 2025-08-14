using System;
using System.Threading;

namespace Crestron.SimplSharp
{
  /// <summary>
  /// Mock implementation of Crestron CCriticalSection for testing purposes
  /// Provides the same public API surface as the real CCriticalSection
  /// </summary>
  public class CCriticalSection : IDisposable
  {
    #region Private Fields

    private readonly object _lockObject = new object();
    private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
    private bool _disposed = false;

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the CCriticalSection class</summary>
    public CCriticalSection()
    {
      // Mock implementation - no actual initialization required
    }

    #endregion

    #region Public Methods

    /// <summary>Enters the critical section</summary>
    public void Enter()
    {
      if (_disposed) throw new ObjectDisposedException(nameof(CCriticalSection));
      Monitor.Enter(_lockObject);
    }

    /// <summary>Tries to enter the critical section</summary>
    /// <returns>True if the critical section was entered successfully</returns>
    public bool TryEnter()
    {
      if (_disposed) return false;
      return Monitor.TryEnter(_lockObject);
    }

    /// <summary>Tries to enter the critical section with a timeout</summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>True if the critical section was entered successfully</returns>
    public bool TryEnter(int timeout)
    {
      if (_disposed) return false;
      return Monitor.TryEnter(_lockObject, timeout);
    }

    /// <summary>Tries to enter the critical section with a TimeSpan timeout</summary>
    /// <param name="timeout">Timeout as TimeSpan</param>
    /// <returns>True if the critical section was entered successfully</returns>
    public bool TryEnter(TimeSpan timeout)
    {
      if (_disposed) return false;
      return Monitor.TryEnter(_lockObject, timeout);
    }

    /// <summary>Leaves the critical section</summary>
    public void Leave()
    {
      if (_disposed) return;
      try
      {
        Monitor.Exit(_lockObject);
      }
      catch (SynchronizationLockException)
      {
        // Ignore if not held by current thread
      }
    }

    /// <summary>Enters a read lock</summary>
    public void EnterReadLock()
    {
      if (_disposed) throw new ObjectDisposedException(nameof(CCriticalSection));
      _readerWriterLock.EnterReadLock();
    }

    /// <summary>Tries to enter a read lock</summary>
    /// <returns>True if the read lock was acquired successfully</returns>
    public bool TryEnterReadLock()
    {
      if (_disposed) return false;
      return _readerWriterLock.TryEnterReadLock(0);
    }

    /// <summary>Tries to enter a read lock with a timeout</summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>True if the read lock was acquired successfully</returns>
    public bool TryEnterReadLock(int timeout)
    {
      if (_disposed) return false;
      return _readerWriterLock.TryEnterReadLock(timeout);
    }

    /// <summary>Exits the read lock</summary>
    public void ExitReadLock()
    {
      if (_disposed) return;
      try
      {
        _readerWriterLock.ExitReadLock();
      }
      catch (SynchronizationLockException)
      {
        // Ignore if not held by current thread
      }
    }

    /// <summary>Enters a write lock</summary>
    public void EnterWriteLock()
    {
      if (_disposed) throw new ObjectDisposedException(nameof(CCriticalSection));
      _readerWriterLock.EnterWriteLock();
    }

    /// <summary>Tries to enter a write lock</summary>
    /// <returns>True if the write lock was acquired successfully</returns>
    public bool TryEnterWriteLock()
    {
      if (_disposed) return false;
      return _readerWriterLock.TryEnterWriteLock(0);
    }

    /// <summary>Tries to enter a write lock with a timeout</summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>True if the write lock was acquired successfully</returns>
    public bool TryEnterWriteLock(int timeout)
    {
      if (_disposed) return false;
      return _readerWriterLock.TryEnterWriteLock(timeout);
    }

    /// <summary>Exits the write lock</summary>
    public void ExitWriteLock()
    {
      if (_disposed) return;
      try
      {
        _readerWriterLock.ExitWriteLock();
      }
      catch (SynchronizationLockException)
      {
        // Ignore if not held by current thread
      }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>Disposes the critical section and releases resources</summary>
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
          _readerWriterLock?.Dispose();
        }

        _disposed = true;
      }
    }

    #endregion
  }
}
