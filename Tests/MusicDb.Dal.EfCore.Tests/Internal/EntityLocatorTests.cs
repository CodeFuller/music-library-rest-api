using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.EfCore.Internal;
using MusicDb.Dal.EfCore.Tests.Utility;

namespace MusicDb.Dal.EfCore.Tests.Internal
{
	[TestClass]
	public class EntityLocatorTests
	{
		private Artist Artist1 => new Artist
		{
			Id = 1,
			Name = "Lacuna Coil",
		};

		private Artist Artist2 => new Artist
		{
			Id = 2,
			Name = "The Gathering",
			Discs = new List<Disc> { Disc1, Disc2 },
		};

		private Disc Disc1 => new Disc
		{
			Id = 11,
			Year = 1998,
			Title = "How To Measure A Planet",
		};

		private Disc Disc2 => new Disc
		{
			Id = 12,
			Year = 2003,
			Title = "Souvenirs",
			Songs = new List<Song> { Song1, Song2 },
		};

		private Song Song1 => new Song
		{
			Id = 101,
			TrackNumber = 1,
			Title = "These Good People",
			Duration = new TimeSpan(0, 5, 48),
		};

		private Song Song2 => new Song
		{
			Id = 102,
			TrackNumber = 2,
			Title = "Even The Spirits Are Afraid",
			Duration = new TimeSpan(0, 5, 12),
		};

		[TestMethod]
		public async Task FindArtist_IfArtistExistsAndIncludeDiscsIsFalse_ReturnsArtistDataWithoutDiscs()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			var artist = await target.FindArtist(2, false, CancellationToken.None);

			// Assert

			Assert.AreEqual(2, artist.Id);
			Assert.AreEqual("The Gathering", artist.Name);
			Assert.IsNull(artist.Discs);
		}

		[TestMethod]
		public async Task FindArtist_IfArtistExistsAndIncludeDiscsIsTrue_ReturnsArtistDataWithDiscs()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			var artist = await target.FindArtist(2, true, CancellationToken.None);

			// Assert

			Assert.AreEqual(2, artist.Id);
			Assert.AreEqual("The Gathering", artist.Name);
			Assert.IsNotNull(artist.Discs);
			var discs = artist.Discs.ToList();
			Assert.AreEqual(2, discs.Count);
			Assert.AreEqual("How To Measure A Planet", discs[0].Title);
			Assert.AreEqual("Souvenirs", discs[1].Title);
		}

		[TestMethod]
		public async Task FindArtist_IfArtistDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.FindArtist(3, false, CancellationToken.None));
		}

		[TestMethod]
		public async Task FindArtistDisc_IfDiscExistsAndIncludeSongsIsFalse_ReturnsDiscDataWithoutSongs()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			var disc = await target.FindArtistDisc(2, 12, false, CancellationToken.None);

			// Assert

			Assert.AreEqual(12, disc.Id);
			Assert.AreEqual(2003, disc.Year);
			Assert.AreEqual("Souvenirs", disc.Title);
			Assert.IsNotNull(disc.Artist);
			Assert.AreEqual(2, disc.Artist.Id);
			Assert.IsNull(disc.Songs);
		}

		[TestMethod]
		public async Task FindArtistDisc_IfDiscExistsAndIncludeSongsIsTrue_ReturnsDiscDataWithSongs()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			var disc = await target.FindArtistDisc(2, 12, true, CancellationToken.None);

			// Assert

			Assert.AreEqual(12, disc.Id);
			Assert.AreEqual(2003, disc.Year);
			Assert.AreEqual("Souvenirs", disc.Title);
			Assert.IsNotNull(disc.Artist);
			Assert.AreEqual(2, disc.Artist.Id);
			Assert.IsNotNull(disc.Songs);
			var songs = disc.Songs.ToList();
			Assert.AreEqual(2, songs.Count);
			Assert.AreEqual("These Good People", songs[0].Title);
			Assert.AreEqual("Even The Spirits Are Afraid", songs[1].Title);
		}

		[TestMethod]
		public async Task FindArtistDisc_IfDiscDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.FindArtistDisc(2, 13, false, CancellationToken.None));
		}

		[TestMethod]
		public async Task FindArtistDisc_IfDiscDoesNotBelongToArtist_ThrowsNotFoundException()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.FindArtistDisc(1, 12, false, CancellationToken.None));
		}

		[TestMethod]
		public async Task FindDiscSong_IfSongExists_ReturnsCorrectSongData()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			var song = await target.FindDiscSong(2, 12, 102, CancellationToken.None);

			// Assert

			Assert.AreEqual(102, song.Id);
			Assert.AreEqual((short)2, song.TrackNumber);
			Assert.AreEqual("Even The Spirits Are Afraid", song.Title);
			Assert.AreEqual(new TimeSpan(0, 5, 12), song.Duration);
			Assert.IsNotNull(song.Disc);
			Assert.AreEqual(12, song.Disc.Id);
		}

		[TestMethod]
		public async Task FindDiscSong_IfSongDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.FindDiscSong(2, 12, 103, CancellationToken.None));
		}

		[TestMethod]
		public async Task FindDiscSong_IfSongDoesNotBelongToDisc_ThrowsNotFoundException()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.FindDiscSong(2, 11, 102, CancellationToken.None));
		}

		[TestMethod]
		public async Task FindDiscSong_IfDiscDoesNotBelongToArtist_ThrowsNotFoundException()
		{
			// Arrange

			var target = CreateTestTarget();

			// Act

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.FindDiscSong(1, 12, 102, CancellationToken.None));
		}

		private EntityLocator CreateTestTarget()
		{
			var(context, options) = Utils.CreateTestContext();

			var seedContext = new MusicDbContext(options);
			seedContext
				.WithArtists(Artist1, Artist2)
				.SaveChanges();

			return new EntityLocator(context);
		}
	}
}
