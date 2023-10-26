using Crestron.SimplSharpPro;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Adds logging to Register() failure
    /// </summary>
    public static class GenericBaseExtensions
    {
        public static eDeviceRegistrationUnRegistrationResponse RegisterWithLogging(this GenericBase device, string key)
        {
            var result = device.Register();
            var level = result == eDeviceRegistrationUnRegistrationResponse.Success ?
                Debug.ErrorLogLevel.Notice : Debug.ErrorLogLevel.Error;
            Debug.Console(0, level, "Register device result: '{0}', type '{1}', result {2}", key, device, result);
            //if (result != eDeviceRegistrationUnRegistrationResponse.Success)
            //{
            //    Debug.Console(0, Debug.ErrorLogLevel.Error, "Cannot register device '{0}': {1}", key, result);
            //}
            return result;
        }

    }
}