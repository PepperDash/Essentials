using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the functionality required to prompt a user to enter a password
    /// </summary>
    public interface IPasswordPrompt
    {
        /// <summary>
        /// Notifies when a password is required or is entered incorrectly
        /// </summary>
        event EventHandler<PasswordPromptEventArgs> PasswordRequired;

        /// <summary>
        /// Submits the password
        /// </summary>
        /// <param name="password">The password to submit</param>
        void SubmitPassword(string password);
    }

    /// <summary>
    /// PasswordPromptEventArgs class
    /// </summary>
    public class PasswordPromptEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates if the last submitted password was incorrect
        /// </summary>
        public bool LastAttemptWasIncorrect { get; private set; }

        /// <summary>
        /// Gets or sets the LoginAttemptFailed
        /// </summary>
        public bool LoginAttemptFailed { get; private set; }

        /// <summary>
        /// Gets or sets the LoginAttemptCancelled
        /// </summary>
        public bool LoginAttemptCancelled { get; private set; }

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lastAttemptIncorrect">indicates if the last submitted password was incorrect</param>
        /// <param name="loginFailed">indicates if the login attempt failed</param>
        /// <param name="loginCancelled">indicates if the login attempt was cancelled</param>
        /// <param name="message">provides a message related to the password prompt</param>
        public PasswordPromptEventArgs(bool lastAttemptIncorrect, bool loginFailed, bool loginCancelled, string message)
        {
            LastAttemptWasIncorrect = lastAttemptIncorrect;
            LoginAttemptFailed = loginFailed;
            LoginAttemptCancelled = loginCancelled;
            Message = message;
        }
    }
}