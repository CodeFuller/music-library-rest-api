using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicDb.Api.IntegrationTests.DataContracts;

namespace MusicDb.Api.IntegrationTests.Controllers
{
	[TestClass]
	public class DiscsControllerTests
	{
		private readonly CustomWebApplicationFactory webApplicationFactory = new CustomWebApplicationFactory();

		[TestInitialize]
		public void Initialize()
		{
			webApplicationFactory.SeedData();
		}

		[TestMethod]
		public async Task GetArtistDiscs_ReturnsCorrectArtistsData()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/api/artists/2/discs", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var discs = await response.Content.ReadAsAsync<List<DiscData>>(CancellationToken.None);
			Assert.AreEqual(2, discs.Count);

			var disc1 = discs[0];
			Assert.AreEqual("Proud Like A God", disc1.Title);
			Assert.AreEqual(1997, disc1.Year);
			var link1 = disc1.Links.Single();
			Assert.AreEqual("self", link1.Rel);
			Assert.AreEqual("http://localhost/api/artists/2/discs/21", link1.Href);

			var disc2 = discs[1];
			Assert.AreEqual("Don't Give Me Names", disc2.Title);
			Assert.IsNull(disc2.Year);
			var link2 = disc2.Links.Single();
			Assert.AreEqual("self", link2.Rel);
			Assert.AreEqual("http://localhost/api/artists/2/discs/22", link2.Href);
		}

		[TestMethod]
		public async Task GetSpecificDisc_ReturnsCorrectDiscData()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/api/artists/2/discs/21", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var disc = await response.Content.ReadAsAsync<DiscData>(CancellationToken.None);

			Assert.AreEqual("Proud Like A God", disc.Title);
			Assert.AreEqual(1997, disc.Year);
			var link = disc.Links.Single();
			Assert.AreEqual("self", link.Rel);
			Assert.AreEqual("http://localhost/api/artists/2/discs/21", link.Href);
		}

		[TestMethod]
		public async Task CreateDisc_CreatesDiscCorrectly()
		{
			// Arrange

			var newDisc = new DiscData
			{
				Title = "Walking On A Thin Line",
				Year = 2003,
			};

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.PostAsJsonAsync(new Uri("/api/artists/2/discs", UriKind.Relative), newDisc);

			// Assert

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2/discs/23", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var createdDisc = await response2.Content.ReadAsAsync<DiscData>(CancellationToken.None);
			Assert.AreEqual("Walking On A Thin Line", createdDisc.Title);
			Assert.AreEqual(2003, createdDisc.Year);
		}

		[TestMethod]
		public async Task UpdateDisc_UpdatesDiscDataCorrectly()
		{
			// Arrange

			var newDiscData = new DiscData
			{
				Title = "Don't Give Me Names (Digipak Version)",
				Year = 2000,
			};

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.PutAsJsonAsync(new Uri("/api/artists/2/discs/22", UriKind.Relative), newDiscData);

			// Assert

			Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2/discs/22", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var updatedDisc = await response2.Content.ReadAsAsync<DiscData>(CancellationToken.None);
			Assert.AreEqual("Don't Give Me Names (Digipak Version)", updatedDisc.Title);
			Assert.AreEqual(2000, updatedDisc.Year);
		}

		[TestMethod]
		public async Task DeleteDisc_DeletesDiscCorrectly()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.DeleteAsync(new Uri("/api/artists/2/discs/22", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2/discs", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var currentDiscs = await response2.Content.ReadAsAsync<List<DiscData>>(CancellationToken.None);

			Assert.AreEqual(1, currentDiscs.Count);

			var disc = currentDiscs.Single();
			Assert.AreEqual("Proud Like A God", disc.Title);
		}
	}
}
