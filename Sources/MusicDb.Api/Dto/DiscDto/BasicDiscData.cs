using System.ComponentModel.DataAnnotations;

namespace MusicDb.Api.Dto.DiscDto
{
	public abstract class BasicDiscData
	{
		[Required]
		public string Title { get; set; }

		public int? Year { get; set; }
	}
}
