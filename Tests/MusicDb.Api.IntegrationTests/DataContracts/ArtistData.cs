﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicDb.Api.IntegrationTests.DataContracts
{
	[DataContract]
	public class ArtistData
	{
		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "links")]
		public ICollection<LinkData> Links { get; set; } = new List<LinkData>();
	}
}
