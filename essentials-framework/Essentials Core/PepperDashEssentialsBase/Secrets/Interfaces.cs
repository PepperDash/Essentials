using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// All ISecrecretProvider classes must implement this interface.
    /// </summary>
    public interface ISecretProvider : IKeyed
    {
        bool SetSecret(string key, object value);

        ISecret GetSecret(string key);
    }

    /// <summary>
    /// interface for delivering secrets in Essentials.
    /// </summary>
    public interface ISecret
    {
        ISecretProvider Provider { get; }
        string Key { get; }
        object Value { get; }
    }
}