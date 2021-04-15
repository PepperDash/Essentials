using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    public class CrestronSecretsProvider : ISecretProvider
    {
        public string Key { get; set; }
        //Added for reference
        //private readonly bool _secureSupported;
        public CrestronSecretsProvider(string key)
        {
            Key = key;
            //Added for future encrypted reference
            //_secureSupported = CrestronSecureStorage.Supported;

            //if (_secureSupported)
            //{
            //      return;
            //}
            CrestronDataStoreStatic.InitCrestronDataStore();

        }


        public void SetSecret(string key, object value)
        {
            var secret = value as string;
            if (String.IsNullOrEmpty(secret))
            {
                Debug.Console(2, this, "Unable to set secret for {0}:{1} - value is empty.", Key, key);
                return;
            }
            Debug.Console(2, this, "Attempting to set Secret to {0}", secret);
            var setErrorCode = CrestronDataStoreStatic.SetLocalStringValue(key, secret);
            switch (setErrorCode)
            {
                case CrestronDataStore.CDS_ERROR.CDS_SUCCESS:
                    Debug.Console(2, this,"Secret Successfully Set for {0}:{1}", Key, key);
                    break;
                default:
                    Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Unable to set secret for {0}:{1} - {2}", Key, key, setErrorCode.ToString());
                    break;
            }
        }

        public ISecret GetSecret(string key)
        {
            string mySecret;
            var getErrorCode = CrestronDataStoreStatic.GetLocalStringValue(key, out mySecret);

            switch (getErrorCode)
            {
                case CrestronDataStore.CDS_ERROR.CDS_SUCCESS:
                    Debug.Console(2, this, "Secret Successfully retrieved for {0}:{1}", Key, key);
                    Debug.Console(2, this, "Retreived Secret = {0}", mySecret);
                    return new CrestronSecret(key, mySecret, this);
                default:
                    Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Unable to retrieve secret for {0}:{1} - {2}",
                        Key, key, getErrorCode.ToString());
                    return null;
            }
        }
    }

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