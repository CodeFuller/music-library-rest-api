using System.Runtime.Serialization;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.SongDto
{
	[DataContract]
	public class InputSongData : BasicSongData
	{
		public Song ToModel(int id = 0)
		{
			return new Song
			{
				Id = id,
				Title = Title,
				TrackNumber = TrackNumber,
				Duration = Duration,
			};
		}
	}
}
