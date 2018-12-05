using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MusicDb.Api.Dto.SongDto
{
	[DataContract]
	public class BasicSongData
	{
		[Required]
		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "trackNumber")]
		public short? TrackNumber { get; set; }

		[DataMember(Name = "duration")]
		public TimeSpan? Duration { get; set; }
	}
}
