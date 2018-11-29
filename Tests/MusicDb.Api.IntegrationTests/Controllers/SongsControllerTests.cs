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
	public class SongsControllerTests
	{
		private readonly CustomWebApplicationFactory webApplicationFactory = new CustomWebApplicationFactory();

		[TestInitialize]
		public void Initialize()
		{
			webApplicationFactory.SeedData();
		}

		[TestMethod]
		public async Task GetAllDiscSongs_ReturnsCorrectSongsData()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/api/artists/2/discs/22/songs", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var songs = await response.Content.ReadAsAsync<List<SongData>>(CancellationToken.None);
			Assert.AreEqual(2, songs.Count);

			var song1 = songs[0];
			Assert.AreEqual("Innocent Greed", song1.Title);
			Assert.AreEqual((short)1, song1.TrackNumber);
			Assert.AreEqual(new TimeSpan(0, 3, 51), song1.Duration);
			var link1 = song1.Links.Single();
			Assert.AreEqual("self", link1.Rel);
			Assert.AreEqual("http://localhost/api/artists/2/discs/22/songs/221", link1.Href);

			var song2 = songs[1];
			Assert.AreEqual("No Speech", song2.Title);
			Assert.IsNull(song2.TrackNumber);
			Assert.IsNull(song2.Duration);
			var link2 = song2.Links.Single();
			Assert.AreEqual("self", link2.Rel);
			Assert.AreEqual("http://localhost/api/artists/2/discs/22/songs/222", link2.Href);
		}

		[TestMethod]
		public async Task GetSpecificSong_ReturnsCorrectSongData()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/api/artists/2/discs/22/songs/221", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var song = await response.Content.ReadAsAsync<SongData>(CancellationToken.None);

			Assert.AreEqual("Innocent Greed", song.Title);
			Assert.AreEqual((short)1, song.TrackNumber);
			Assert.AreEqual(new TimeSpan(0, 3, 51), song.Duration);
			var link = song.Links.Single();
			Assert.AreEqual("self", link.Rel);
			Assert.AreEqual("http://localhost/api/artists/2/discs/22/songs/221", link.Href);
		}

		[TestMethod]
		public async Task CreateSong_CreatesSongCorrectly()
		{
			// Arrange

			var newSong = new SongData
			{
				Title = "Big In Japan",
				TrackNumber = 3,
				Duration = new TimeSpan(0, 2, 48),
			};

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.PostAsJsonAsync(new Uri("/api/artists/2/discs/22/songs", UriKind.Relative), newSong);

			// Assert

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2/discs/22/songs/223", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var createdSong = await response2.Content.ReadAsAsync<SongData>(CancellationToken.None);
			Assert.AreEqual("Big In Japan", createdSong.Title);
			Assert.AreEqual((short)3, createdSong.TrackNumber);
			Assert.AreEqual(new TimeSpan(0, 2, 48), createdSong.Duration);
		}

		[TestMethod]
		public async Task UpdateSong_UpdatesSongDataCorrectly()
		{
			// Arrange

			var newSongData = new SongData
			{
				Title = "No Speech (Bonus)",
				TrackNumber = 13,
				Duration = new TimeSpan(0, 13, 33),
			};

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.PutAsJsonAsync(new Uri("/api/artists/2/discs/22/songs/222", UriKind.Relative), newSongData);

			// Assert

			Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2/discs/22/songs/222", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var updatedSong = await response2.Content.ReadAsAsync<SongData>(CancellationToken.None);
			Assert.AreEqual("No Speech (Bonus)", updatedSong.Title);
			Assert.AreEqual((short)13, updatedSong.TrackNumber);
			Assert.AreEqual(new TimeSpan(0, 13, 33), updatedSong.Duration);
		}

		[TestMethod]
		public async Task DeleteSong_DeletesSongCorrectly()
		{
			// Arrange

			var client = webApplicationFactory.CreateClient();

			// Act

			var response = await client.DeleteAsync(new Uri("/api/artists/2/discs/22/songs/222", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

			var response2 = await client.GetAsync(new Uri("/api/artists/2/discs/22/songs", UriKind.Relative));
			Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
			var currentSongs = await response2.Content.ReadAsAsync<List<SongData>>(CancellationToken.None);

			Assert.AreEqual(1, currentSongs.Count);

			var song = currentSongs.Single();
			Assert.AreEqual("Innocent Greed", song.Title);
		}
	}
}
