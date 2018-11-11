using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer.Repositories;

namespace MusicDb.Dal.SqlServer.Tests.Repositories
{
	[TestClass]
	public class ArtistsRepositoryTests
	{
		[TestMethod]
		public async Task CreateArtist_IfArtistWithSuchNameDoesNotExist_AddsArtistSuccessfully()
		{
			// Arrange

			var artist = new Artist
			{
				Name = "Nautilus Pompilius",
			};

			(var target, var options) = CreateTestTarget();

			// Act

			await target.CreateArtist(artist, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreEqual(1, context.Artists.Count());
			Assert.AreEqual("Nautilus Pompilius", context.Artists.Single().Name);
		}

		[TestMethod]
		public async Task CreateArtist_IfArtistWasCreated_FillsArtistId()
		{
			// Arrange

			var artist = new Artist
			{
				Name = "Lacuna Coil",
			};

			(var target, var options) = CreateTestTarget();

			// Act

			await target.CreateArtist(artist, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreNotEqual(0, artist.Id);
			Assert.AreEqual(context.Artists.Single().Id, artist.Id);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
		public async Task CreateArtist_IfArtistWithNameAlreadyExist_Throws()
		{
			// Arrange

			var prevArtist = new Artist
			{
				Name = "Louna",
			};

			var newArtist = new Artist
			{
				Name = "Louna",
			};

			(var target, _) = CreateTestTarget();

			await target.CreateArtist(prevArtist, CancellationToken.None);

			// Act & Assert

			await target.CreateArtist(newArtist, CancellationToken.None);
		}

		[TestMethod]
		public async Task GetArtists_IfSomeArtistsExist_ReturnsAllArtists()
		{
			// Arrange

			var artist1 = new Artist
			{
				Name = "The Gathering",
			};

			var artist2 = new Artist
			{
				Name = "Korn",
			};

			(var target, _) = CreateTestTarget();
			await target.CreateArtist(artist1, CancellationToken.None);
			await target.CreateArtist(artist2, CancellationToken.None);

			// Act

			var artists = (await target.GetAllArtists(CancellationToken.None))
				.ToList();

			// Assert

			Assert.AreEqual(2, artists.Count);
			Assert.AreEqual("The Gathering", artists[0].Name);
			Assert.AreEqual("Korn", artists[1].Name);
		}

		[TestMethod]
		public async Task GetArtists_IfNoArtistsExist_ReturnsEmptyCollection()
		{
			// Arrange

			(var target, _) = CreateTestTarget();

			// Act

			var artists = await target.GetAllArtists(CancellationToken.None);

			// Assert

			Assert.AreEqual(0, artists.Count);
		}

		[TestMethod]
		public async Task GetArtist_IfArtistExist_ReturnsCorrectArtistData()
		{
			// Arrange

			var existingArtist1 = new Artist
			{
				Name = "Epica",
			};

			var existingArtist2 = new Artist
			{
				Name = "Within Temptation",
			};

			(var target, _) = CreateTestTarget();
			await target.CreateArtist(existingArtist1, CancellationToken.None);
			await target.CreateArtist(existingArtist2, CancellationToken.None);

			// Act

			var foundArtist = await target.GetArtist(2, CancellationToken.None);

			// Assert

			Assert.AreEqual("Within Temptation", foundArtist.Name);
		}

		[TestMethod]
		public async Task GetArtist_IfArtistDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			var existingArtist = new Artist
			{
				Name = "Guano Apes",
			};

			(var target, _) = CreateTestTarget();
			await target.CreateArtist(existingArtist, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.GetArtist(2, CancellationToken.None));
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistExist_UpdatesArtistDataCorrectly()
		{
			// Arrange

			var existingArtist1 = new Artist
			{
				Name = "Linkin Park",
			};

			var existingArtist2 = new Artist
			{
				Name = "Prince",
			};

			var updatedArtist = new Artist
			{
				Id = 2,
				Name = "The Artist Formerly Known as Prince",
			};

			(var target, var options) = CreateTestTarget();
			await target.CreateArtist(existingArtist1, CancellationToken.None);
			await target.CreateArtist(existingArtist2, CancellationToken.None);

			// Act

			await target.UpdateArtist(updatedArtist, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var checkedArtist = context.Artists.Single(x => x.Id == 2);
			Assert.AreEqual("The Artist Formerly Known as Prince", checkedArtist.Name);
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			var existingArtist = new Artist
			{
				Name = "Nightwish",
			};

			var updatedArtist = new Artist
			{
				Id = 2,
				Name = "The Nightwish",
			};

			(var target, _) = CreateTestTarget();
			await target.CreateArtist(existingArtist, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.UpdateArtist(updatedArtist, CancellationToken.None));
		}

		[TestMethod]
		public async Task DeleteArtist_IfArtistExist_DeletesArtistCorrectly()
		{
			// Arrange

			var existingArtist1 = new Artist
			{
				Name = "Lacrimosa",
			};

			var existingArtist2 = new Artist
			{
				Name = "Limp Bizkit",
			};

			(var target, var options) = CreateTestTarget();
			await target.CreateArtist(existingArtist1, CancellationToken.None);
			await target.CreateArtist(existingArtist2, CancellationToken.None);

			// Act

			await target.DeleteArtist(2, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreEqual(1, context.Artists.Count());
			Assert.AreEqual("Lacrimosa", context.Artists.Single().Name);
		}

		[TestMethod]
		public async Task DeleteArtist_IfArtistDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			var existingArtist = new Artist
			{
				Name = "P.O.D.",
			};

			(var target, _) = CreateTestTarget();
			await target.CreateArtist(existingArtist, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.DeleteArtist(2, CancellationToken.None));
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private static (ArtistsRepository, DbContextOptions<MusicDbContext>) CreateTestTarget()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			var connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();

			var options = new DbContextOptionsBuilder<MusicDbContext>()
				.UseSqlite(connection)
				.Options;

			using (var initContext = new MusicDbContext(options))
			{
				initContext.Database.EnsureCreated();
			}

			var context = new MusicDbContext(options);
			return (new ArtistsRepository(context), options);
		}
	}
}
