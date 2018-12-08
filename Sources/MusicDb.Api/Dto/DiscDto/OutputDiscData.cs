using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicDb.Api.Dto.DiscDto
{
	[DataContract]
	public class OutputDiscData : BasicDiscData
	{
		[DataMember(Name = "links")]
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();
	}
}
