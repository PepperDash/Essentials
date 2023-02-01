using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Core.Web
{
	public class EssentialsWebApiHelpers
	{
		public static object MapToAssemblyObject(LoadedAssembly assembly)
		{
			return new
			{
				Name = assembly.Name,
				Version = assembly.Version
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
				Joins = join.Value.Joins.Select(j => MapJoinDatacompleteToObject(j))
			};
		}

		public static object MapJoinDatacompleteToObject(KeyValuePair<string, JoinDataComplete> joinData)
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
	}
}