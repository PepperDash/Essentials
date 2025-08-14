using System;
using System.Threading;

namespace Crestron.SimplSharp
{
  /// <summary>Mock timer event handler</summary>
  /// <param name="userObject">User-defined object</param>
  public delegate void CTimerEventHandler(object? userObject);

  /// <summary>
  /// Mock implementation of Crestron CTimer for testing purposes
  /// Provides the same public API surface as the real CTimer
  /// </summary>
  public class CTimer : IDisposable
  {
    #region Private Fields

    private Timer? _timer;
    private readonly object _lockObject = new object();
    private bool _disposed = false;
    private readonly CTimerEventHandler? _callback;
    private readonly object? _userObject;

    #endregion

    #region Properties

    /// <summary>Gets whether the timer is currently running</summary>
    public bool Running { get; private set; } = false;

    /// <summary>Gets the timer interval in milliseconds</summary>
    public long TimeToFire { get; private set; } = 0;

    #endregion

    #region Constructors

    /// <summary>Initializes a new instance of the CTimer class</summary>
    /// <param name="callbackFunction">Function to call when timer fires</param>
    /// <param name="userObject">User-defined object to pass to callback</param>
    /// <param name="dueTime">Time before timer first fires (milliseconds)</param>
    /// <param name="period">Interval between timer fires (milliseconds), or -1 for one-shot</param>
    public CTimer(CTimerEventHandler callbackFunction, object? userObject, long dueTime, long period)
    {
      _callback = callbackFunction;
      _userObject = userObject;
      TimeToFire = period;

      var dueTimeInt = dueTime > int.MaxValue ? Timeout.Infinite : (int)dueTime;
      var periodInt = period > int.MaxValue || period < 0 ? Timeout.Infinite : (int)period;

      _timer = new Timer(TimerCallback, null, dueTimeInt, periodInt);
      Running = dueTime != Timeout.Infinite;
    }

    /// <summary>Initializes a new instance of the CTimer class</summary>
    /// <param name="callbackFunction">Function to call when timer fires</param>
    /// <param name="userObject">User-defined object to pass to callback</param>
    /// <param name="dueTime">Time before timer first fires (milliseconds)</param>
    public CTimer(CTimerEventHandler callbackFunction, object? userObject, long dueTime)
        : this(callbackFunction, userObject, dueTime, -1)
    {
    }

    /// <summary>Initializes a new instance of the CTimer class</summary>
    /// <param name="callbackFunction">Function to call when timer fires</param>
    /// <param name="userObject">User-defined object to pass to callback</param>
    public CTimer(CTimerEventHandler callbackFunction, object? userObject)
        : this(callbackFunction, userObject, Timeout.Infinite, -1)
    {
    }

    /// <summary>Initializes a new instance of the CTimer class</summary>
    /// <param name="callbackFunction">Function to call when timer fires</param>
    public CTimer(CTimerEventHandler callbackFunction)
        : this(callbackFunction, null, Timeout.Infinite, -1)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>Resets the timer with a new due time</summary>
    /// <param name="dueTime">New due time in milliseconds</param>
    /// <returns>True if successful</returns>
    public bool Reset(long dueTime)
    {
      return Reset(dueTime, -1);
    }

    /// <summary>Resets the timer with new due time and period</summary>
    /// <param name="dueTime">New due time in milliseconds</param>
    /// <param name="period">New period in milliseconds, or -1 for one-shot</param>
    /// <returns>True if successful</returns>
    public bool Reset(long dueTime, long period)
    {
      lock (_lockObject)
      {
        if (_disposed || _timer == null) return false;

        TimeToFire = period;
        var dueTimeInt = dueTime > int.MaxValue ? Timeout.Infinite : (int)dueTime;
        var periodInt = period > int.MaxValue || period < 0 ? Timeout.Infinite : (int)period;

        try
        {
          _timer.Change(dueTimeInt, periodInt);
          Running = dueTime != Timeout.Infinite;
          return true;
        }
        catch
        {
          return false;
        }
      }
    }

    /// <summary>Stops the timer</summary>
    /// <returns>True if successful</returns>
    public bool Stop()
    {
      lock (_lockObject)
      {
        if (_disposed || _timer == null) return false;

        try
        {
          _timer.Change(Timeout.Infinite, Timeout.Infinite);
          Running = false;
          return true;
        }
        catch
        {
          return false;
        }
      }
    }

    #endregion

    #region Private Methods

    /// <summary>Internal timer callback</summary>
    /// <param name="state">Timer state (unused)</param>
    private void TimerCallback(object? state)
    {
      try
      {
        _callback?.Invoke(_userObject);
      }
      catch
      {
        // Suppress exceptions in callback to prevent timer from stopping
      }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>Disposes the timer and releases resources</summary>
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
          lock (_lockObject)
          {
            _timer?.Dispose();
            _timer = null;
            Running = false;
          }
        }

        _disposed = true;
      }
    }

    #endregion
  }
}
