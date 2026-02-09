using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using PepperDash.Core;
using Crestron.SimplSharpPro;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a CrestronLocalSecretsProvider
    /// </summary>
    public class CrestronLocalSecretsProvider : ISecretProvider
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }
        //Added for reference
        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Constructor for CrestronLocalSecretsProvider
        /// </summary>
        /// <param name="key">The key for the secret provider</param>
        public CrestronLocalSecretsProvider(string key)
        {
            Key = key;
            Description = String.Format("Default secret provider serving Essentials Application {0}", InitialParametersClass.ApplicationNumber);
        }

        static CrestronLocalSecretsProvider()
        {
            //Added for future encrypted reference
            var secureSupported = CrestronSecureStorage.Supported;

            CrestronDataStoreStatic.InitCrestronDataStore();
            if (secureSupported)
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
                if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                {
                    Debug.LogMessage(LogEventLevel.Information, this, "Successfully removed secret \"{0}\"", secret);
                    return true;
                }
            }

            else  
            {
                returnCode = CrestronDataStoreStatic.SetLocalStringValue(key, secret);
                if (returnCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                {
                    Debug.LogMessage(LogEventLevel.Information, this, "Successfully set secret \"{0}\"", secret);
                    return true;
                }
            }

            Debug.LogMessage(LogEventLevel.Information, this, "Unable to set secret for {0}:{1} - {2}", Key, key, returnCode.ToString());
            return false; 
        }

        /// <summary>
        /// Retrieve secret for item in the CrestronSecretsProvider
        /// </summary>
        /// <param name="key">Secret Key</param>
        /// <returns>ISecret Object containing key, provider, and value</returns>
        /// <summary>
        /// GetSecret method
        /// </summary>
        public ISecret GetSecret(string key)
        {
            string mySecret;
            var getErrorCode = CrestronDataStoreStatic.GetLocalStringValue(key, out mySecret);

            switch (getErrorCode)
            {
                case CrestronDataStore.CDS_ERROR.CDS_SUCCESS:
                    Debug.LogMessage(LogEventLevel.Verbose, this, "Secret Successfully retrieved for {0}:{1}", Key, key);
                    return new CrestronSecret(key, mySecret, this);
                default:
                    Debug.LogMessage(LogEventLevel.Information, this, "Unable to retrieve secret for {0}:{1} - {2}",
                        Key, key, getErrorCode.ToString());
                    return null;
            }
        }

        /// <summary>
        /// Determine if a secret is present within the provider without retrieving it
        /// </summary>
        /// <param name="key">Secret Key</param>
        /// <returns>bool if present</returns>
        /// <summary>
        /// TestSecret method
        /// </summary>
        public bool TestSecret(string key)
        {
            string mySecret;
            return CrestronDataStoreStatic.GetLocalStringValue(key, out mySecret) == CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
        }
    }

}