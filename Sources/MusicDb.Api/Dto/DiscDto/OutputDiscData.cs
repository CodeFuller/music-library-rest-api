using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.DiscDto
{
	[DataContract]
	public class OutputDiscData : BasicDiscData
	{
		[DataMember(Name = "links")]
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();

		public OutputDiscData(Disc disc, Uri location)
		{
			Title = disc.Title;
			Year = disc.Year;
			Links.Add(LinkDto.Self(location));
		}
	}
}
