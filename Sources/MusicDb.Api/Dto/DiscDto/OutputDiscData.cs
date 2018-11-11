using System;
using System.Collections.Generic;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.DiscDto
{
	public class OutputDiscData : BasicDiscData
	{
		public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();

		public OutputDiscData(Disc disc, Uri location)
		{
			Title = disc.Title;
			Year = disc.Year;
			Links.Add(LinkDto.Self(location));
		}
	}
}
