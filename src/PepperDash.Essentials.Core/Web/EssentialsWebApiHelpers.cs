using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Web
{
	/// <summary>
	/// EssentialsWebApiHelpers class
	/// </summary>
	public static class EssentialsWebApiHelpers
	{
		/// <summary>
		/// GetRequestBody method
		/// </summary>
		public static string GetRequestBody(this HttpCwsRequest request)
		{
			var bytes = new byte[request.ContentLength];

			request.InputStream.Read(bytes, 0, request.ContentLength);

			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// MapToAssemblyObject method
		/// </summary>
		public static object MapToAssemblyObject(LoadedAssembly assembly)
		{
			return new
			{
                assembly.Name,
                assembly.Version
			};
		}

		/// <summary>
		/// MapToDeviceListObject method
		/// </summary>
		public static object MapToDeviceListObject(IKeyed device)
		{
			return new
			{
                device.Key,
				Name = (device is IKeyName)
					? (device as IKeyName).Name
					: "---"
			};
		}

		/// <summary>
		/// MapJoinToObject method
		/// </summary>
		public static object MapJoinToObject(string key, JoinMapBaseAdvanced join)
		{
			var kp = new KeyValuePair<string, JoinMapBaseAdvanced>(key, join);

			return MapJoinToObject(kp);
		}

		/// <summary>
		/// MapJoinToObject method
		/// </summary>
		public static object MapJoinToObject(KeyValuePair<string, JoinMapBaseAdvanced> join)
		{
			return new
			{
				DeviceKey = join.Key,
				Joins = join.Value.Joins.Select(j => MapJoinDataCompleteToObject(j))
			};
		}

		/// <summary>
		/// MapJoinDataCompleteToObject method
		/// </summary>
		public static object MapJoinDataCompleteToObject(KeyValuePair<string, JoinDataComplete> joinData)
		{
			return new
			{
				Signal = joinData.Key,
				Description = joinData.Value.Metadata.Description,
				JoinNumber = joinData.Value.JoinNumber,
				JoinSpan = joinData.Value.JoinSpan,
				JoinType = joinData.Value.Metadata.JoinType.ToString(),
				JoinCapabilities = joinData.Value.Metadata.JoinCapabilities.ToString()
			};
		}

		/// <summary>
		/// MapDeviceTypeToObject method
		/// </summary>
		public static object MapDeviceTypeToObject(string key, DeviceFactoryWrapper device)
		{
			var kp = new KeyValuePair<string, DeviceFactoryWrapper>(key, device);

			return MapDeviceTypeToObject(kp);
		}

		/// <summary>
		/// MapDeviceTypeToObject method
		/// </summary>
		public static object MapDeviceTypeToObject(KeyValuePair<string, DeviceFactoryWrapper> device)
		{
			return new
			{
				Type = device.Key,
				Description = device.Value.Description,
				CType = device.Value.Type == null ? "---": device.Value.Type.ToString()
			};
		}
	}
}