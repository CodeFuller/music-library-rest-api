using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicDb.Api.IntegrationTests.DataContracts
{
	[DataContract]
	public class DiscData
	{
		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "year")]
		public int? Year { get; set; }

		[DataMember(Name = "links")]
		public ICollection<LinkData> Links { get; set; } = new List<LinkData>();
	}
}
