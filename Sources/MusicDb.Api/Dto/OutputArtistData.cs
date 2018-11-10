using System;
using System.Collections.Generic;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto
{
	public class OutputArtistData : BasicArtistData
	{
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();

		public OutputArtistData(Artist artist, Uri location)
		{
			Name = artist.Name;
			Links.Add(LinkDto.Self(location));
		}
	}
}
