using System;
using System.Collections.Generic;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.SongDto
{
	public class OutputSongData : BasicSongData
	{
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();

		public OutputSongData(Song song, Uri location)
		{
			Title = song.Title;
			TrackNumber = song.TrackNumber;
			Duration = song.Duration;
			Links.Add(LinkDto.Self(location));
		}
	}
}
