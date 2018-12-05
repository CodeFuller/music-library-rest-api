using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MusicDb.Api.Dto.DiscDto
{
	[DataContract]
	public abstract class BasicDiscData
	{
		[Required]
		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "year")]
		public int? Year { get; set; }
	}
}
