using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.SongDto
{
	[DataContract]
	public class OutputSongData : BasicSongData
	{
		[DataMember(Name = "links")]
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
