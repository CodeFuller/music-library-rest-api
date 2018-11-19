using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer.Repositories;
using MusicDb.Dal.SqlServer.Tests.Utility;

namespace MusicDb.Dal.SqlServer.Tests.Repositories
{
	[TestClass]
	public class DiscsRepositoryTests
	{
		private Artist Artist => new Artist
		{
			Id = 1,
			Name = "Guano Apes",
		};

		private Disc Disc1 => new Disc
		{
			Year = 1997,
			Title = "Proud Like A God",
		};

		private Disc Disc2 => new Disc
		{
			Year = 2000,
			Title = "Don't Give Me Names",
		};

		[TestMethod]
		public async Task CreateDisc_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			// Act

			await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtist(Artist.Id, true, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateDisc_ForExistingArtist_CreatesDiscSuccessfully()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			// Act

			await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var addedDisc = context.Discs
				.Include(d => d.Artist)
				.Single();
			Assert.AreEqual(Disc1.Title, addedDisc.Title);
			Assert.AreEqual(Disc1.Year, addedDisc.Year);
			Assert.AreEqual(Artist.Id, addedDisc.Artist.Id);
		}

		[TestMethod]
		public async Task CreateDisc_IfDiscWasCreated_ReturnsIdOfNewDisc()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			// Act

			var discId = await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreNotEqual(0, discId);
			Assert.AreEqual(context.Discs.Single().Id, discId);
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			// Act

			await target.GetAllArtistDiscs(Artist.Id, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtist(Artist.Id, true, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistHasSomeDiscs_ReturnsAllArtistDiscs()
		{
			// Arrange

			var(target, _, _) = CreateTestTarget();

			await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);
			await target.CreateDisc(Artist.Id, Disc2, CancellationToken.None);

			// Act

			var discs = (await target.GetAllArtistDiscs(Artist.Id, CancellationToken.None))
				.ToList();

			// Assert

			Assert.AreEqual(2, discs.Count);
			Assert.AreEqual(Disc1.Title, discs[0].Title);
			Assert.AreEqual(Disc2.Title, discs[1].Title);
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistHasNoDiscs_ReturnsEmptyCollection()
		{
			// Arrange

			var(target, _, _) = CreateTestTarget();

			// Act

			var discs = (await target.GetAllArtistDiscs(Artist.Id, CancellationToken.None))
				.ToList();

			// Assert

			Assert.IsFalse(discs.Any());
		}

		[TestMethod]
		public async Task GetDisc_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorStub) = CreateTestTarget();

			var disc = new Disc();

			entityLocatorStub.Setup(x => x.FindArtistDisc(Artist.Id, 123, false, It.IsAny<CancellationToken>()))
				.ReturnsAsync(disc);

			// Act

			var returnedDisc = await target.GetDisc(Artist.Id, 123, CancellationToken.None);

			// Assert

			Assert.AreSame(disc, returnedDisc);
		}

		[TestMethod]
		public async Task UpdateDisc_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			var discId = await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			var disc = Disc1;
			disc.Id = discId;

			// Act

			await target.UpdateDisc(Artist.Id, disc, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtistDisc(Artist.Id, discId, false, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateDisc_IfDiscExists_UpdateDiscDataCorrectly()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			var discId = await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			var newDiscData = new Disc
			{
				Id = discId,
				Title = "Some new title",
				Year = 2018,
			};

			// Act

			await target.UpdateDisc(Artist.Id, newDiscData, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var updatedDisc = context.Discs.Single();
			Assert.AreEqual(discId, updatedDisc.Id);
			Assert.AreEqual("Some new title", updatedDisc.Title);
			Assert.AreEqual(2018, updatedDisc.Year);
		}

		[TestMethod]
		public async Task DeleteDisc_InvokesEntityLocatorCorrectly()
		{
			// Arrange

			var(target, _, entityLocatorMock) = CreateTestTarget();

			var discId = await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			// Act

			await target.DeleteDisc(Artist.Id, discId, CancellationToken.None);

			// Assert

			entityLocatorMock.Verify(x => x.FindArtistDisc(Artist.Id, discId, false, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteDisc_IfDiscExists_DeletesDisc()
		{
			// Arrange

			var(target, options, _) = CreateTestTarget();

			var discId = await target.CreateDisc(Artist.Id, Disc1, CancellationToken.None);

			// Act

			await target.DeleteDisc(Artist.Id, discId, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.IsFalse(context.Discs.Any());
			var artist = context.Artists
				.Include(a => a.Discs)
				.Single();
			Assert.IsFalse(artist.Discs.Any());
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private (DiscsRepository, DbContextOptions<MusicDbContext>, Mock<IEntityLocator>) CreateTestTarget()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			var entityLocatorStub = new Mock<IEntityLocator>();

			var(context, options) = Utils.CreateTestContext();

			var seedContext = new MusicDbContext(options);
			seedContext
				.WithArtists(Artist)
				.SaveChanges();

			entityLocatorStub.Setup(x => x.FindArtist(It.IsAny<int>(), true, It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
					context.Artists
						.Include(a => a.Discs)
						.Single());

			entityLocatorStub.Setup(x => x.FindArtistDisc(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => context.Discs.Single());

			return (new DiscsRepository(context, entityLocatorStub.Object), options, entityLocatorStub);
		}
	}
}
