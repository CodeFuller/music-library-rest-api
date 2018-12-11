using System;
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
	public class ArtistsRepositoryTests
	{
		private Artist Artist => new Artist
		{
			Id = 1,
			Name = "Nautilus Pompilius",
		};

		[TestMethod]
		public async Task CreateArtist_IfArtistWithSuchNameDoesNotExist_AddsArtistSuccessfully()
		{
			// Arrange

			var artist = new Artist
			{
				Name = "Nautilus Pompilius",
			};

			var(target, options, _) = CreateTestTarget();

			// Act

			await target.CreateArtist(artist, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreEqual(1, context.Artists.Count());
			Assert.AreEqual("Nautilus Pompilius", context.Artists.Single().Name);
		}

		[TestMethod]
		public async Task CreateArtist_IfArtistWasCreated_ReturnsIdOfNewArtist()
		{
			// Arrange

			var artist = new Artist
			{
				Name = "Lacuna Coil",
			};

			var(target, options, _) = CreateTestTarget();

			// Act

			var newId = await target.CreateArtist(artist, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreNotEqual(0, newId);
			Assert.AreEqual(context.Artists.Single().Id, newId);
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

			var(target, _, _) = CreateTestTarget();

			await target.CreateArtist(prevArtist, CancellationToken.None);

			// Act & Assert

			await target.CreateArtist(newArtist, CancellationToken.None);
		}

		[TestMethod]
		public async Task GetAllArtists_IfSomeArtistsExist_ReturnsAllArtists()
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

			var(target, _, _) = CreateTestTarget();
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
		public async Task GetAllArtists_IfNoArtistsExist_ReturnsEmptyCollection()
		{
			// Arrange

			var(target, _, _) = CreateTestTarget();

			// Act

			var artists = await target.GetAllArtists(CancellationToken.None);

			// Assert

			Assert.IsFalse(artists.Any());
		}

		[TestMethod]
		public async Task GetArtist_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorStub) = CreateTestTarget();

			var artist = new Artist();

			entityLocatorStub.Setup(x => x.FindArtist(123, false, It.IsAny<CancellationToken>()))
				.ReturnsAsync(artist);

			// Act

			var returnedArtist = await target.GetArtist(123, CancellationToken.None);

			// Assert

			Assert.AreSame(artist, returnedArtist);
		}

		[TestMethod]
		public async Task UpdateArtist_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			var artistId = await target.CreateArtist(Artist, CancellationToken.None);

			var artist = Artist;
			artist.Id = artistId;

			// Act

			await target.UpdateArtist(artist, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtist(artistId, false, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistExists_UpdateArtistDataCorrectly()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			var aritstId = await target.CreateArtist(Artist, CancellationToken.None);

			var newArtistData = new Artist
			{
				Id = aritstId,
				Name = "Some new name",
			};

			// Act

			await target.UpdateArtist(newArtistData, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var updatedArtist = context.Artists.Single();
			Assert.AreEqual(aritstId, updatedArtist.Id);
			Assert.AreEqual("Some new name", updatedArtist.Name);
		}

		[TestMethod]
		public async Task DeleteArtist_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			var artistId = await target.CreateArtist(Artist, CancellationToken.None);

			// Act

			await target.DeleteArtist(artistId, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtist(artistId, false, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteArtist_IfArtistExists_DeletesArtist()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			var artistId = await target.CreateArtist(Artist, CancellationToken.None);

			// Act

			await target.DeleteArtist(artistId, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.IsFalse(context.Artists.Any());
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private static (ArtistsRepository, DbContextOptions<MusicDbContext>, Mock<IEntityLocator>) CreateTestTarget()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			var entityLocatorStub = new Mock<IEntityLocator>();

			var(context, options) = Utils.CreateTestContext();

			entityLocatorStub.Setup(x => x.FindArtist(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => context.Artists.Single());

			return (new ArtistsRepository(context, entityLocatorStub.Object), options, entityLocatorStub);
		}
	}
}
