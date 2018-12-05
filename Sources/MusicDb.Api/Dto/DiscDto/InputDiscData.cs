using System.Runtime.Serialization;
using MusicDb.Abstractions.Models;

namespace MusicDb.Api.Dto.DiscDto
{
	[DataContract]
	public class InputDiscData : BasicDiscData
	{
		public Disc ToModel(int id = 0)
		{
			return new Disc
			{
				Id = id,
				Title = Title,
				Year = Year,
			};
		}
	}
}
