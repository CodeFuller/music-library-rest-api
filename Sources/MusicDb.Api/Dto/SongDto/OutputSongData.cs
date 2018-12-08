using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicDb.Api.Dto.SongDto
{
	[DataContract]
	public class OutputSongData : BasicSongData
	{
		[DataMember(Name = "links")]
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();
	}
}
