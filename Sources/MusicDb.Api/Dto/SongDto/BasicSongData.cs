using System;
using System.ComponentModel.DataAnnotations;

namespace MusicDb.Api.Dto.SongDto
{
	public class BasicSongData
	{
		[Required]
		public string Title { get; set; }

		public short? TrackNumber { get; set; }

		public TimeSpan? Duration { get; set; }
	}
}
