using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public interface ISecretProvider : IKeyed
    {
        void SetSecret(string key, object value);

        ISecret GetSecret(string key);
    }

    public interface ISecret
    {
        ISecretProvider Provider { get; }
        string Key { get; }
        object Value { get; }
    }
}