using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.EfCore.Repositories;
using MusicDb.Dal.EfCore.Tests.Utility;

namespace MusicDb.Dal.EfCore.Tests.Repositories
{
	[TestClass]
	public class SongsRepositoryTests
	{
		private Artist Artist => new Artist
		{
			Id = 1,
			Name = "Korn",
			Discs = new Collection<Disc> { Disc },
		};

		private Disc Disc => new Disc
		{
			Id = 22,
			Year = 1998,
			Title = "Follow The Leader",
		};

		private Song Song1 => new Song
		{
			TrackNumber = 1,
			Title = "It's On!",
			Duration = new TimeSpan(0, 4, 28),
		};

		private Song Song2 => new Song
		{
			TrackNumber = 2,
			Title = "Freak On A Leash",
			Duration = new TimeSpan(0, 4, 15),
		};

		[TestMethod]
		public async Task CreateSong_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			// Act

			await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtistDisc(Artist.Id, Disc.Id, true, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateSong_ForExistingArtistDisc_CreatesSongSuccessfully()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			// Act

			await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var addedSong = context.Songs
				.Include(d => d.Disc)
				.Single();
			Assert.AreEqual(Song1.Title, addedSong.Title);
			Assert.AreEqual(Song1.TrackNumber, addedSong.TrackNumber);
			Assert.AreEqual(Disc.Id, addedSong.Disc.Id);
		}

		[TestMethod]
		public async Task CreateSong_IfSongWasCreated_ReturnsIdOfNewSong()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			// Act

			var songId = await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreNotEqual(0, songId);
			Assert.AreEqual(context.Songs.Single().Id, songId);
		}

		[TestMethod]
		public async Task GetAllDiscSongs_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			// Act

			await target.GetAllDiscSongs(Artist.Id, Disc.Id, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtistDisc(Artist.Id, Disc.Id, true, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task GetAllDiscSongs_IfDiscHasSomeSongs_ReturnsAllDiscSongs()
		{
			// Arrange

			var(target, _, _) = CreateTestTarget();

			await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);
			await target.CreateSong(Artist.Id, Disc.Id, Song2, CancellationToken.None);

			// Act

			var songs = (await target.GetAllDiscSongs(Artist.Id, Disc.Id, CancellationToken.None))
				.ToList();

			// Assert

			Assert.AreEqual(2, songs.Count);
			Assert.AreEqual(Song1.Title, songs[0].Title);
			Assert.AreEqual(Song2.Title, songs[1].Title);
		}

		[TestMethod]
		public async Task GetAllDiscSongs_IfDiscHasNoSongs_ReturnsEmptyCollection()
		{
			// Arrange

			var(target, _, _) = CreateTestTarget();

			// Act

			var songs = (await target.GetAllDiscSongs(Artist.Id, Disc.Id, CancellationToken.None))
				.ToList();

			// Assert

			Assert.IsFalse(songs.Any());
		}

		[TestMethod]
		public async Task GetSong_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorStub) = CreateTestTarget();

			var song = new Song();

			entityLocatorStub.Setup(x => x.FindDiscSong(Artist.Id, Disc.Id, 123, It.IsAny<CancellationToken>()))
				.ReturnsAsync(song);

			// Act

			var returnedSong = await target.GetSong(Artist.Id, Disc.Id, 123, CancellationToken.None);

			// Assert

			Assert.AreSame(song, returnedSong);
		}

		[TestMethod]
		public async Task UpdateSong_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			var songId = await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			var song = Song1;
			song.Id = songId;

			// Act

			await target.UpdateSong(Artist.Id, Disc.Id, song, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindDiscSong(Artist.Id, Disc.Id, songId, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateSong_IfSongExists_UpdateSongDataCorrectly()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			var songId = await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			var newSongData = new Song
			{
				Id = songId,
				Title = "Some new title",
				TrackNumber = 123,
				Duration = TimeSpan.FromSeconds(12345),
			};

			// Act

			await target.UpdateSong(Artist.Id, Disc.Id, newSongData, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var updatedSong = context.Songs.Single();
			Assert.AreEqual(songId, updatedSong.Id);
			Assert.AreEqual("Some new title", updatedSong.Title);
			Assert.AreEqual((short)123, updatedSong.TrackNumber);
			Assert.AreEqual(TimeSpan.FromSeconds(12345), updatedSong.Duration);
		}

		[TestMethod]
		public async Task DeleteSong_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			var songId = await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			// Act

			await target.DeleteSong(Artist.Id, Disc.Id, songId, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindDiscSong(Artist.Id, Disc.Id, songId, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteSong_IfSongExists_DeletesSong()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			var songId = await target.CreateSong(Artist.Id, Disc.Id, Song1, CancellationToken.None);

			// Act

			await target.DeleteSong(Artist.Id, Disc.Id, songId, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.IsFalse(context.Songs.Any());
			var disc = context.Discs
				.Include(a => a.Songs)
				.Single();
			Assert.IsFalse(disc.Songs.Any());
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private (SongsRepository, DbContextOptions<MusicDbContext>, Mock<IEntityLocator>) CreateTestTarget()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			var entityLocatorStub = new Mock<IEntityLocator>();

			var(context, options) = Utils.CreateTestContext();

			var seedContext = new MusicDbContext(options);
			seedContext
				.WithArtists(Artist)
				.SaveChanges();

			entityLocatorStub.Setup(x => x.FindArtistDisc(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
					context.Discs
						.Include(d => d.Songs)
						.Single());

			entityLocatorStub.Setup(x => x.FindDiscSong(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => context.Songs.Single());

			return (new SongsRepository(context, entityLocatorStub.Object), options, entityLocatorStub);
		}
	}
}
