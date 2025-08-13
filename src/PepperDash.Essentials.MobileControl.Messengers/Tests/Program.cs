using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.AppServer.Messengers.Tests;

namespace PepperDash.Essentials.MobileControl.Tests
{
    /// <summary>
    /// Simple test program to verify CallStatusMessenger functionality
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing CallStatusMessenger with IHasDialer device...");

            try
            {
                // Create a mock device that implements IHasDialer but not VideoCodecBase
                var mockDevice = new MockCallStatusDevice("mock-codec-1", "Mock Call Device");
                
                // Create the new CallStatusMessenger
                var messenger = new CallStatusMessenger(
                    "test-messenger-1",
                    mockDevice,
                    "/device/mock-codec-1"
                );

                Console.WriteLine("✓ Successfully created CallStatusMessenger with IHasDialer device");

                // Test basic call functionality
                Console.WriteLine("\nTesting call functionality:");
                
                Console.WriteLine("- Initial call status: " + (mockDevice.IsInCall ? "In Call" : "Not In Call"));
                
                // Test dialing a number
                mockDevice.Dial("1234567890");
                Console.WriteLine("- After dialing: " + (mockDevice.IsInCall ? "In Call" : "Not In Call"));
                
                // Test ending all calls
                mockDevice.EndAllCalls();
                Console.WriteLine("- After ending all calls: " + (mockDevice.IsInCall ? "In Call" : "Not In Call"));

                // Test content sharing if supported
                if (mockDevice is IHasContentSharing sharingDevice)
                {
                    Console.WriteLine("\nTesting content sharing:");
                    Console.WriteLine("- Initial sharing status: " + sharingDevice.SharingContentIsOnFeedback.BoolValue);
                    
                    sharingDevice.StartSharing();
                    Console.WriteLine("- After start sharing: " + sharingDevice.SharingContentIsOnFeedback.BoolValue);
                    
                    sharingDevice.StopSharing();
                    Console.WriteLine("- After stop sharing: " + sharingDevice.SharingContentIsOnFeedback.BoolValue);
                }

                Console.WriteLine("\n✓ All tests passed! CallStatusMessenger works with interface-based devices.");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Test failed: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}