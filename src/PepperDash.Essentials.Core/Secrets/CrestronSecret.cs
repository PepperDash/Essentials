namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Special container class for CrestronSecret provider
    /// </summary>
    public class CrestronSecret : ISecret
    {
        public ISecretProvider Provider { get; private set; }
        public string Key { get; private set; }

        public object Value { get; private set; }

        public CrestronSecret(string key, string value, ISecretProvider provider)
        {
            Key = key;
            Value = value;
            Provider = provider;
        }

    }

}