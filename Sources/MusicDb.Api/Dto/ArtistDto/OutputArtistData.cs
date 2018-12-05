using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.ArtistDto
{
	[DataContract]
	public class OutputArtistData : BasicArtistData
	{
		[DataMember(Name = "links")]
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();

		public OutputArtistData(Artist artist, Uri location)
		{
			Name = artist.Name;
			Links.Add(LinkDto.Self(location));
		}
	}
}
