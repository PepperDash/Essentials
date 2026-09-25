using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// A store of per-person room setups: who is using the room, what they had saved for a room
    /// like this one, and a way to save it again.
    ///
    /// <para>Implemented by a device that fronts a profile database. The device owns the
    /// credentials, the transport and the schema; everything above it sees a person's setup as a
    /// bag of named values and never learns where it came from.</para>
    ///
    /// <para><b>Values are strings, and absence is meaningful.</b> A profile row is shared by
    /// every room of a given type, so a room must be able to say "I have no opinion about this"
    /// as distinct from "set this to zero" — a room with no mobile cart that wrote a default
    /// would clear that column for that person in every room that does have one. A key that is
    /// absent from <see cref="SaveUserPreset"/> is therefore left exactly as stored. Implementers
    /// must read-modify-write rather than replacing the row.</para>
    ///
    /// <para>What the keys mean is the store's and its caller's business, not this interface's.
    /// They are the column names of whatever database is behind it, and both sides have to agree
    /// on them the way they would agree on any other wire contract.</para>
    /// </summary>
    public interface IHasUserPresets
    {
        /// <summary>
        /// Raised when the store has an answer about the current person — normally in response to
        /// <see cref="UserLoggedIn"/>, but a store may push one at any time.
        ///
        /// <para>An answer carrying <see cref="UserPresetLoadedEventArgs.IsStored"/> false is a
        /// real answer: the person was found and has nothing saved for a room of this type. A
        /// caller can act on that at once rather than waiting out whatever timeout it keeps for a
        /// store that has gone quiet, which is the difference between a first-time user waiting
        /// and a first-time user being handed the room's defaults.</para>
        /// </summary>
        event EventHandler<UserPresetLoadedEventArgs> UserPresetLoaded;

        /// <summary>
        /// Raised when a save finishes, successfully or not, so a caller can say so rather than
        /// leaving somebody to guess whether their settings were kept.
        /// </summary>
        event EventHandler<UserPresetSavedEventArgs> UserPresetSaved;

        /// <summary>
        /// Tell the store who is using the room. It looks them up and raises
        /// <see cref="UserPresetLoaded"/> either way.
        /// </summary>
        /// <param name="username">The person's identifier in the profile database.</param>
        void UserLoggedIn(string username);

        /// <summary>Tell the store the room is empty again.</summary>
        void UserLoggedOut();

        /// <summary>
        /// Save these values against the person who is currently logged in, leaving every value
        /// not named here as it is stored.
        /// </summary>
        /// <param name="values">
        /// The values to write. A key absent from this collection is not changed; see the
        /// interface remarks.
        /// </param>
        void SaveUserPreset(IDictionary<string, string> values);
    }

    /// <summary>
    /// The store's answer about the person currently using the room.
    /// </summary>
    public class UserPresetLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPresetLoadedEventArgs"/> class.
        /// </summary>
        /// <param name="username">Who the answer is about, or null if nobody was found.</param>
        /// <param name="isStored">Whether these values were saved by that person.</param>
        /// <param name="values">Their values, keyed by the store's own column names.</param>
        public UserPresetLoadedEventArgs(string username, bool isStored,
            IReadOnlyDictionary<string, string> values)
        {
            Username = username;
            IsStored = isStored;
            Values = values ?? new Dictionary<string, string>();
        }

        /// <summary>Gets who the answer is about, or null if nobody was found.</summary>
        public string Username { get; }

        /// <summary>
        /// Gets a value indicating whether this person had actually saved these values.
        ///
        /// <para>False means the store produced something usable — its own defaults — rather than
        /// reading a saved row. A caller that treats a default as somebody's saved room will
        /// overwrite their real one the next time it saves.</para>
        /// </summary>
        public bool IsStored { get; }

        /// <summary>Gets their values, keyed by the store's own column names.</summary>
        public IReadOnlyDictionary<string, string> Values { get; }
    }

    /// <summary>
    /// How a save turned out.
    /// </summary>
    public class UserPresetSavedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPresetSavedEventArgs"/> class.
        /// </summary>
        /// <param name="succeeded">Whether the store kept the values.</param>
        /// <param name="message">Why it did not, in terms a person can act on.</param>
        public UserPresetSavedEventArgs(bool succeeded, string message = null)
        {
            Succeeded = succeeded;
            Message = message;
        }

        /// <summary>Gets a value indicating whether the store kept the values.</summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets why the save did not succeed, in terms a person can act on, or null when it did.
        /// </summary>
        public string Message { get; }
    }
}
