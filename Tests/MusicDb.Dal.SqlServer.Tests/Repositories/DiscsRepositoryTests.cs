using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer.Repositories;
using MusicDb.Dal.SqlServer.Tests.Utility;

namespace MusicDb.Dal.SqlServer.Tests.Repositories
{
	[TestClass]
	public class DiscsRepositoryTests
	{
		private Artist Artist1 => new Artist
		{
			Id = 1,
			Name = "Korn",
		};

		private Artist Artist2 => new Artist
		{
			Id = 2,
			Name = "Lacuna Coil",
		};

		private Disc Disc11 => new Disc
		{
			Year = 1999,
			Title = "Issues",
		};

		private Disc Disc21 => new Disc
		{
			Year = 2008,
			Title = "Shallow Life",
		};

		private Disc Disc22 => new Disc
		{
			Year = 2014,
			Title = "Broken Crown Halo",
		};

		[TestMethod]
		public async Task CreateDisc_IfArtistExists_CreatesDiscSuccessfully()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			// Act

			await target.CreateDisc(Artist2.Id, Disc21, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var addedDisc = context.Discs
				.Include(d => d.Artist)
				.Single();
			Assert.AreEqual(Disc21.Title, addedDisc.Title);
			Assert.AreEqual(Disc21.Year, addedDisc.Year);
			Assert.AreEqual(Artist2.Id, addedDisc.Artist.Id);
		}

		[TestMethod]
		public async Task CreateDisc_IfDiscWasCreated_ReturnsIdOfNewDisc()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1)
				.Save();

			// Act

			var discId = await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			Assert.AreNotEqual(0, discId);
			Assert.AreEqual(context.Discs.Single().Id, discId);
		}

		[TestMethod]
		public async Task CreateDisc_IfArtistDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1)
				.Save();

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.CreateDisc(Artist2.Id, Disc21, CancellationToken.None));
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistHasSomeDiscs_ReturnsAllArtistDiscs()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);
			await target.CreateDisc(Artist2.Id, Disc21, CancellationToken.None);
			await target.CreateDisc(Artist2.Id, Disc22, CancellationToken.None);

			// Act

			var discs = (await target.GetAllArtistDiscs(2, CancellationToken.None))
				.ToList();

			// Assert

			Assert.AreEqual(2, discs.Count);
			Assert.AreEqual(Disc21.Title, discs[0].Title);
			Assert.AreEqual(Disc22.Title, discs[1].Title);
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistHasNoDiscs_ReturnsEmptyCollection()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			// Act

			var discs = await target.GetAllArtistDiscs(Artist2.Id, CancellationToken.None);

			// Assert

			Assert.IsFalse(discs.Any());
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1)
				.Save();

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.GetAllArtistDiscs(Artist2.Id, CancellationToken.None));
		}

		[TestMethod]
		public async Task GetDisc_IfDiscExists_ReturnsDiscData()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);
			await target.CreateDisc(Artist2.Id, Disc21, CancellationToken.None);
			var discId = await target.CreateDisc(Artist2.Id, Disc22, CancellationToken.None);

			// Act

			var disc = await target.GetDisc(Artist2.Id, discId, CancellationToken.None);

			// Assert

			Assert.AreEqual(Disc22.Title, disc.Title);
		}

		[TestMethod]
		public async Task GetDisc_IfDiscDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.GetDisc(Artist1.Id, 777, CancellationToken.None));
		}

		[TestMethod]
		public async Task GetDisc_IfDiscDoesNotBelongToArtist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			var discId = await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.GetDisc(Artist2.Id, discId, CancellationToken.None));
		}

		[TestMethod]
		public async Task UpdateDisc_IfDiscExists_UpdateDiscDataCorrectly()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);
			await target.CreateDisc(Artist2.Id, Disc21, CancellationToken.None);
			var discId = await target.CreateDisc(Artist2.Id, Disc22, CancellationToken.None);

			var newDiscData = new Disc
			{
				Id = discId,
				Title = "Some new title",
				Year = 2018,
			};

			// Act

			await target.UpdateDisc(Artist2.Id, newDiscData, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var updatedDisc = context.Discs.Single(d => d.Id == discId);
			Assert.AreEqual("Some new title", updatedDisc.Title);
			Assert.AreEqual(2018, updatedDisc.Year);
		}

		[TestMethod]
		public async Task UpdateDisc_IfDiscDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			var newDiscData = new Disc
			{
				Id = 777,
				Title = "Some new title",
				Year = 2018,
			};

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.UpdateDisc(Artist1.Id, newDiscData, CancellationToken.None));
		}

		[TestMethod]
		public async Task UpdateDisc_IfDiscDoesNotBelongToArtist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			var discId = await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			var newDiscData = new Disc
			{
				Id = discId,
				Title = "Some new title",
				Year = 2018,
			};

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.UpdateDisc(Artist2.Id, newDiscData, CancellationToken.None));
		}

		[TestMethod]
		public async Task DeleteDisc_IfDiscExists_DeletesDisc()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);
			await target.CreateDisc(Artist2.Id, Disc21, CancellationToken.None);
			var discId = await target.CreateDisc(Artist2.Id, Disc22, CancellationToken.None);

			// Act

			await target.DeleteDisc(Artist2.Id, discId, CancellationToken.None);

			// Assert

			var context = new MusicDbContext(options);
			var artist = context.Artists
				.Include(a => a.Discs)
				.Single(d => d.Id == Artist2.Id);
			Assert.AreEqual(1, artist.Discs.Count);
			Assert.AreEqual(Disc21.Title, artist.Discs.Single().Title);
		}

		[TestMethod]
		public async Task DeleteDisc_IfDiscDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1)
				.Save();

			await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.DeleteDisc(Artist1.Id, 777, CancellationToken.None));
		}

		[TestMethod]
		public async Task DeleteDisc_IfDiscDoesNotBelongToArtist_ThrowsNotFoundException()
		{
			// Arrange

			(var target, var options) = CreateTestTarget();

			options.ToContext()
				.WithArtists(Artist1, Artist2)
				.Save();

			var discId = await target.CreateDisc(Artist1.Id, Disc11, CancellationToken.None);

			// Act & Assert

			await Assert.ThrowsExceptionAsync<NotFoundException>(() => target.DeleteDisc(Artist2.Id, discId, CancellationToken.None));
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private static (DiscsRepository, DbContextOptions<MusicDbContext>) CreateTestTarget()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			(var context, var options) = Utils.CreateTestContext();
			return (new DiscsRepository(context), options);
		}
	}
}
