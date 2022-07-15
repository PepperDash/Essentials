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
        private static readonly bool SecureSupported;
        public CrestronSecretsProvider(string key)
        {
            Key = key;
        }

        static CrestronSecretsProvider()
        {
            //Added for future encrypted reference
            SecureSupported = CrestronSecureStorage.Supported;

            CrestronDataStoreStatic.InitCrestronDataStore();
            if (SecureSupported)
            {
                //doThingsFuture
            }
        }

        /// <summary>
        /// Set secret for item in the CrestronSecretsProvider
        /// </summary>
        /// <param name="key">Secret Key</param>
        /// <param name="value">Secret Value</param>
        public bool SetSecret(string key, object value)
        {
            var secret = value as string;
            CrestronDataStore.CDS_ERROR returnCode;

            if (String.IsNullOrEmpty(secret))
            {
                returnCode = CrestronDataStoreStatic.clearLocal(key);
                if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS) return true;
            }

            else  
            {
                returnCode = CrestronDataStoreStatic.SetLocalStringValue(key, secret);
                if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS) return true;
            }

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Unable to set secret for {0}:{1} - {2}", Key, key, returnCode.ToString());
            return false; 
        }

        /// <summary>
        /// Retrieve secret for item in the CrestronSecretsProvider
        /// </summary>
        /// <param name="key">Secret Key</param>
        /// <returns>ISecret Object containing key, provider, and value</returns>
        public ISecret GetSecret(string key)
        {
            string mySecret;
            var getErrorCode = CrestronDataStoreStatic.GetLocalStringValue(key, out mySecret);

            switch (getErrorCode)
            {
                case CrestronDataStore.CDS_ERROR.CDS_SUCCESS:
                    Debug.Console(2, this, "Secret Successfully retrieved for {0}:{1}", Key, key);
                    return new CrestronSecret(key, mySecret, this);
                default:
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Unable to retrieve secret for {0}:{1} - {2}",
                        Key, key, getErrorCode.ToString());
                    return null;
            }
        }
    }

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