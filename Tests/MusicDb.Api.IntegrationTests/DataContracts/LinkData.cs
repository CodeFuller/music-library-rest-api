using System.Runtime.Serialization;

namespace MusicDb.Api.IntegrationTests.DataContracts
{
	[DataContract]
	public class LinkData
	{
		[DataMember(Name = "rel")]
		public string Rel { get; set; }

		[DataMember(Name = "href")]
		public string Href { get; set; }
	}
}
