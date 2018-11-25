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
	public class ArtistsControllerTests
	{
		private readonly CustomWebApplicationFactory webApplicationFactory = new CustomWebApplicationFactory();

		[TestInitialize]
		public void Initialize()
		{
			webApplicationFactory.SeedData();
		}

		[TestMethod]
		public async Task GetArtists_ReturnsCorrectArtistsData()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/api/artists", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var artists = await response.Content.ReadAsAsync<List<ArtistData>>(CancellationToken.None);
			Assert.AreEqual(2, artists.Count);

			var artist1 = artists[0];
			Assert.AreEqual("Korn", artist1.Name);
			var link1 = artist1.Links.Single();
			Assert.AreEqual("self", link1.Rel);
			Assert.AreEqual("http://localhost/api/artists/1", link1.Href);

			var artist2 = artists[1];
			Assert.AreEqual("Guano Apes", artist2.Name);
			var link2 = artist2.Links.Single();
			Assert.AreEqual("self", link2.Rel);
			Assert.AreEqual("http://localhost/api/artists/2", link2.Href);
		}

		[TestMethod]
		public async Task GetSpecificArtist_ReturnsCorrectArtistData()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/api/artists/2", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var artist = await response.Content.ReadAsAsync<ArtistData>(CancellationToken.None);

			Assert.AreEqual("Guano Apes", artist.Name);
			var link = artist.Links.Single();
			Assert.AreEqual("self", link.Rel);
			Assert.AreEqual("http://localhost/api/artists/2", link.Href);
		}

		[TestMethod]
		public async Task CreateArtist_CreatesArtistCorrectly()
		{
			// Arrange

			var newArtist = new ArtistData
			{
				Name = "Neuro Dubel",
			};

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.PostAsJsonAsync(new Uri("/api/artists", UriKind.Relative), newArtist);

			// Assert

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/3", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var createdArtist = await response2.Content.ReadAsAsync<ArtistData>(CancellationToken.None);
			Assert.AreEqual("Neuro Dubel", createdArtist.Name);
		}

		[TestMethod]
		public async Task UpdateArtist_UpdatesArtistDataCorrectly()
		{
			// Arrange

			var newArtistData = new ArtistData
			{
				Name = "New Guano Apes",
			};

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.PutAsJsonAsync(new Uri("/api/artists/2", UriKind.Relative), newArtistData);

			// Assert

			Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var updatedArtist = await response2.Content.ReadAsAsync<ArtistData>(CancellationToken.None);
			Assert.AreEqual("New Guano Apes", updatedArtist.Name);
		}

		[TestMethod]
		public async Task DeleteArtist_UpdatesArtistDataCorrectly()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.DeleteAsync(new Uri("/api/artists/2", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var currentArtists = await response2.Content.ReadAsAsync<List<ArtistData>>(CancellationToken.None);

			Assert.AreEqual(1, currentArtists.Count);

			var artist = currentArtists.Single();
			Assert.AreEqual("Korn", artist.Name);
		}
	}
}
