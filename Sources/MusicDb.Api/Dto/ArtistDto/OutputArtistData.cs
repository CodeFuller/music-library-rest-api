using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicDb.Api.Dto.ArtistDto
{
	[DataContract]
	public class OutputArtistData : BasicArtistData
	{
		[DataMember(Name = "links")]
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();
	}
}
