using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto
{
	public class InputArtistData : BasicArtistData
	{
		public Artist ToModel(int id = 0)
		{
			return new Artist
			{
				Id = id,
				Name = Name
			};
		}
	}
}
