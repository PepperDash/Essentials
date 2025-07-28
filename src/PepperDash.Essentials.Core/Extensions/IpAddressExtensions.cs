using System;
using System.Net;

namespace PepperDash.Essentials.Core.Extensions
{
    /// <summary>
    /// Extensions for IPAddress to provide additional functionality such as getting broadcast address, network address, and checking if two addresses are in the same subnet.
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// Get the broadcast address for a given IP address and subnet mask.
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <param name="subnetMask">Subnet mask in a.b.c.d format</param>
        /// <returns>Broadcast address</returns>
        /// <remarks>
        /// If the input IP address is 192.168.1.100 and the subnet mask is 255.255.255.0, the broadcast address will be 192.168.1.255
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | subnetMaskBytes[i] ^ 255);
            }
            return new IPAddress(broadcastAddress);
        }

        /// <summary>
        /// Get the network address for a given IP address and subnet mask.
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <param name="subnetMask">Subnet mask in a.b.c.d</param>
        /// <returns>Network Address</returns>
        /// /// <remarks>
        /// If the input IP address is 192.168.1.100 and the subnet mask is 255.255.255.0, the network address will be 192.168.1.0
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & subnetMaskBytes[i]);
            }
            return new IPAddress(broadcastAddress);
        }

        /// <summary>
        /// Determine if two IP addresses are in the same subnet.
        /// </summary>
        /// <param name="address2">Address to check</param>
        /// <param name="address">Second address to check</param>
        /// <param name="subnetMask">Subnet mask to use to compare the 2 IP Address</param>
        /// <returns>True if addresses are in the same subnet</returns>
        /// <remarks>
        /// If the input IP addresses are 192.168.1.100 and 192.168.1.200, and the subnet mask is 255.255.255.0, this will return true.
        /// If the input IP addresses are 10.1.1.100 and 192.168.1.100, and the subnet mask is 255.255.255.0, this will return false.
        /// </remarks>
        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            var network1 = address.GetNetworkAddress(subnetMask);
            var network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }
    }
}
