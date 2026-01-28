using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.DeviceInfo
{
    /// <summary>
    /// Static class NetworkDeviceHelpers
    /// </summary>
    public static class NetworkDeviceHelpers
    {
        /// <summary>
        /// Event raised when ArpTable changes
        /// </summary>
        public static event ArpTableEventHandler ArpTableUpdated;

        /// <summary>
        /// Delegate called by ArpTableUpdated
        /// </summary>
        /// <param name="args">contains the entire ARP table and a bool to note if there was an error in retrieving the data</param>
        public delegate void ArpTableEventHandler(ArpTableEventArgs args);

        private static readonly char NewLineSplitter = CrestronEnvironment.NewLine.ToCharArray().First();
        private static readonly string NewLine = CrestronEnvironment.NewLine;

        private static readonly CCriticalSection Lock = new CCriticalSection();

        /// <summary>
        /// Gets or sets the ArpTable
        /// </summary>
        public static List<ArpEntry> ArpTable { get; private set; }

        /// <summary>
        /// RefreshArp method
        /// </summary>
        public static void RefreshArp()
        {
            var error = false;
            try
            {
                Lock.Enter();
                var consoleResponse = string.Empty;
                if (!CrestronConsole.SendControlSystemCommand("showarptable", ref consoleResponse)) return;
                if (string.IsNullOrEmpty(consoleResponse))
                {
                    error = true;
                    return;
                }
                ArpTable.Clear();

                Debug.LogMessage(LogEventLevel.Verbose, "ConsoleResponse of 'showarptable' : {0}{1}", NewLine, consoleResponse);

                var myLines =
                    consoleResponse.Split(NewLineSplitter)
                        .ToList()
                        .Where(o => (o.Contains(':') && !o.Contains("Type", StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                foreach (var line in myLines)
                {
                    var item = line;
                    var seperator = item.Contains('\t') ? '\t' : ' ';
                    var dataPoints = item.Split(seperator);
                    if (dataPoints == null || dataPoints.Length < 2) continue;
                    var ipAddress = SanitizeIpAddress(dataPoints.First().TrimAll());
                    var macAddress = dataPoints.Last();
                    ArpTable.Add(new ArpEntry(ipAddress, macAddress));
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Information, "Exception in \"RefreshArp\" : {0}", ex.Message);
                error = true;
            }
            finally
            {
                Lock.Leave();
                OnArpTableUpdated(new ArpTableEventArgs(ArpTable, error));
            }
        }


        private static void OnArpTableUpdated(ArpTableEventArgs args)
        {
            if (args == null) return;
            var handler = ArpTableUpdated;
            if (handler == null) return;
            handler.Invoke(args);
        }

        static NetworkDeviceHelpers()
        {
            ArpTable = new List<ArpEntry>();
        }

        /// <summary>
        /// Removes leading zeros, leading whitespace, and trailing whitespace from an IPAddress string
        /// </summary>
        /// <param name="ipAddressIn">Ip Address to Santitize</param>
        /// <returns>Sanitized Ip Address</returns>
        /// <summary>
        /// SanitizeIpAddress method
        /// </summary>
        public static string SanitizeIpAddress(string ipAddressIn)
        {
            try
            {
                var ipAddress = IPAddress.Parse(ipAddressIn.TrimStart('0'));
                return ipAddress.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Information, "Unable to Santize Ip : {0}", ex.Message);
                return ipAddressIn;
            }
        }

        /// <summary>
        /// Resolves a hostname by IP Address using DNS
        /// </summary>
        /// <param name="ipAddress">IP Address to resolve from</param>
        /// <returns>Resolved Hostname - on failure to determine hostname, will return IP Address</returns>
        /// <summary>
        /// ResolveHostnameFromIp method
        /// </summary>
        public static string ResolveHostnameFromIp(string ipAddress)
        {
            try
            {
                var santitizedIp = SanitizeIpAddress(ipAddress);
                var hostEntry = Dns.GetHostEntry(santitizedIp);
                return hostEntry == null ? ipAddress : hostEntry.HostName;
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Information, "Exception Resolving Hostname from IP Address : {0}", ex.Message);
                return ipAddress;
            }
        }

        /// <summary>
        /// Resolves an IP Address by hostname using DNS
        /// </summary>
        /// <param name="hostName">Hostname to resolve from</param>
        /// <returns>Resolved IP Address - on a failure to determine IP Address, will return hostname</returns>
        /// <summary>
        /// ResolveIpFromHostname method
        /// </summary>
        public static string ResolveIpFromHostname(string hostName)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(hostName);
                return hostEntry == null ? hostName : hostEntry.AddressList.First().ToString();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Information, "Exception Resolving IP Address from Hostname : {0}", ex.Message);
                return hostName;
            }
        }

    }

    /// <summary>
    /// Represents a ArpEntry
    /// </summary>
    public class ArpEntry
    {
        /// <summary>
        /// The IP Address of the ARP Entry
        /// </summary>
        public readonly IPAddress IpAddress;

        /// <summary>
        /// The MAC Address of the ARP Entry
        /// </summary>
        public readonly string MacAddress;

        /// <summary>
        /// Constructs new ArpEntry object
        /// </summary>
        /// <param name="ipAddress">string formatted as ipv4 address</param>
        /// <param name="macAddress">mac address string - format is unimportant</param>
        public ArpEntry(string ipAddress, string macAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentException("\"ipAddress\" cannot be null or empty");
            }
            if (string.IsNullOrEmpty(macAddress))
            {
                throw new ArgumentException("\"macAddress\" cannot be null or empty");
            }
            IpAddress = IPAddress.Parse(ipAddress.TrimStart().TrimStart('0').TrimEnd());
            MacAddress = macAddress;
        }
    }

    /// <summary>
    /// Represents a ArpTableEventArgs
    /// </summary>
    public class ArpTableEventArgs : EventArgs
    {
        /// <summary>
        /// The retrieved ARP Table
        /// </summary>
        public readonly List<ArpEntry> ArpTable;
        /// <summary>
        /// True if there was a problem retrieving the ARP Table
        /// </summary>
        public readonly bool Error;

        /// <summary>
        /// Constructor for ArpTableEventArgs
        /// </summary>
        /// <param name="arpTable">The entirety of the retrieved ARP table</param>
        /// <param name="error">True of an error was encountered updating the ARP table</param>
        public ArpTableEventArgs(List<ArpEntry> arpTable, bool error)
        {
            ArpTable = arpTable;
            Error = error;
        }

        /// <summary>
        /// Constructor for ArpTableEventArgs - assumes no error encountered in retrieving ARP Table
        /// </summary>
        /// <param name="arpTable">The entirety of the retrieved ARP table</param>
        public ArpTableEventArgs(List<ArpEntry> arpTable)
        {
            ArpTable = arpTable;
            Error = false;
        }
    }
}