using System;

namespace PepperDash.Essentials.Core.Tests.Abstractions
{
    /// <summary>
    /// Abstraction for thread operations to enable testing
    /// </summary>
    public interface IThreadService
    {
        /// <summary>
        /// Sleeps the current thread for the specified milliseconds
        /// </summary>
        /// <param name="milliseconds">Time to sleep in milliseconds</param>
        void Sleep(int milliseconds);
        
        /// <summary>
        /// Creates and starts a new thread
        /// </summary>
        /// <param name="threadFunction">The function to execute in the thread</param>
        /// <param name="parameter">Parameter to pass to the thread function</param>
        /// <returns>Thread identifier or handle</returns>
        object CreateAndStartThread(Func<object, object> threadFunction, object parameter);
        
        /// <summary>
        /// Aborts the specified thread
        /// </summary>
        /// <param name="thread">The thread to abort</param>
        void AbortThread(object thread);
        
        /// <summary>
        /// Checks if the thread is currently running
        /// </summary>
        /// <param name="thread">The thread to check</param>
        /// <returns>True if the thread is running, false otherwise</returns>
        bool IsThreadRunning(object thread);
    }
}