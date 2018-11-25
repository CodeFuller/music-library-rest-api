using System.Runtime.Serialization;

namespace MusicDb.Api.IntegrationTests.DataContracts
{
	[DataContract]
	public class LinkData
	{
		[DataMember]
		public string Rel { get; set; }

		[DataMember]
		public string Href { get; set; }
	}
}
