using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicDb.Api.Tests
{
	[TestClass]
	public class ApiMappingProfileTests
	{
		[TestMethod]
		public void ApiMappingProfile_FillsMappingsCorrectly()
		{
			// Arrange

			// Act

			Mapper.Initialize(cfg => cfg.AddProfile<ApiMappingProfile>());

			// Assert

			Mapper.AssertConfigurationIsValid();
		}
	}
}
