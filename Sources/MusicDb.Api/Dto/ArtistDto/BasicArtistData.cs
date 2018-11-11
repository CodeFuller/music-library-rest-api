using System.ComponentModel.DataAnnotations;

namespace MusicDb.Api.Dto.ArtistDto
{
	public abstract class BasicArtistData
	{
		[Required]
		public string Name { get; set; }
	}
}
