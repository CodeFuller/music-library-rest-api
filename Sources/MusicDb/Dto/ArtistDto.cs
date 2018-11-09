using System.ComponentModel.DataAnnotations;

namespace MusicDb.Dto
{
	public class ArtistDto
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
