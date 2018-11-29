using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicDb.Api.IntegrationTests.DataContracts
{
	[DataContract]
	public class SongData
	{
		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "trackNumber")]
		public short? TrackNumber { get; set; }

		[DataMember(Name = "duration")]
		public TimeSpan? Duration { get; set; }

		[DataMember(Name = "links")]
		public ICollection<LinkData> Links { get; set; } = new List<LinkData>();
	}
}
