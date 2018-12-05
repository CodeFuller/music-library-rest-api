using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MusicDb.Api.Dto.ArtistDto
{
	[DataContract]
	public abstract class BasicArtistData
	{
		[Required]
		[DataMember(Name = "name")]
		public string Name { get; set; }
	}
}
