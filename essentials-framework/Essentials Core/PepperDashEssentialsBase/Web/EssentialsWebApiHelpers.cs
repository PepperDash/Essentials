﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Web
{
	public class EssentialsWebApiHelpers
	{
		public static string GetRequestBody(HttpCwsRequest request)
		{
			var bytes = new Byte[request.ContentLength];

			request.InputStream.Read(bytes, 0, request.ContentLength);

			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		public static object MapToAssemblyObject(LoadedAssembly assembly)
		{
			return new
			{
				Name = assembly.Name,
				Version = assembly.Version
			};
		}

		public static object MapToDeviceListObject(IKeyed device)
		{
			return new
			{
				Key = device.Key,
				Name = (device is IKeyName)
					? (device as IKeyName).Name
					: "---"
			};
		}

		public static object MapJoinToObject(string key, JoinMapBaseAdvanced join)
		{
			var kp = new KeyValuePair<string, JoinMapBaseAdvanced>(key, join);

			return MapJoinToObject(kp);
		}

		public static object MapJoinToObject(KeyValuePair<string, JoinMapBaseAdvanced> join)
		{
			return new
			{
				DeviceKey = join.Key,
				Joins = join.Value.Joins.Select(j => MapJoinDataCompleteToObject(j))
			};
		}

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

		public static object MapDeviceTypeToObject(string key, DeviceFactoryWrapper device)
		{
			var kp = new KeyValuePair<string, DeviceFactoryWrapper>(key, device);

			return MapDeviceTypeToObject(kp);
		}

		public static object MapDeviceTypeToObject(KeyValuePair<string, DeviceFactoryWrapper> device)
		{
			return new
			{
				Type = device.Key,
				Description = device.Value.Description,
				CType = device.Value.CType == null ? "---": device.Value.CType.ToString()
			};
		}
	}
}