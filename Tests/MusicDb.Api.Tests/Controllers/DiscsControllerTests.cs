using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;
using MusicDb.Api.Controllers;
using MusicDb.Api.Dto.DiscDto;

namespace MusicDb.Api.Tests.Controllers
{
	[TestClass]
	public class DiscsControllerTests
	{
		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistWasFound_ReturnsCorrectDiscsData()
		{
			// Arrange

			var artist = new Artist
			{
				Id = 123,
				Name = "Neuro Dubel",
			};

			var discs = new[]
			{
				new Disc
				{
					Id = 456,
					Year = 2004,
					Title = "Tanki",
					Artist = artist,
				},

				new Disc
				{
					Id = 789,
					Title = "Stasi",
					Artist = artist,
				},
			};

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetAllArtistDiscs(777, It.IsAny<CancellationToken>()))
				.ReturnsAsync(discs);

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetAllArtistDiscs(777, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as IEnumerable<OutputDiscData>;
			Assert.IsNotNull(data);
			var dataList = data.ToList();
			Assert.AreEqual(2, dataList.Count);

			var disc1 = dataList[0];
			Assert.AreEqual("Tanki", disc1.Title);
			Assert.AreEqual(2004, disc1.Year);
			var link1 = disc1.Links.Single();
			Assert.AreEqual("self", link1.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link1.Uri);

			var disc2 = dataList[1];
			Assert.AreEqual("Stasi", disc2.Title);
			Assert.IsNull(disc2.Year);
			var link2 = disc1.Links.Single();
			Assert.AreEqual("self", link2.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link2.Uri);
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetAllArtistDiscs(777, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetAllArtistDiscs(777, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task GetDisc_IfDiscWasFound_ReturnsCorrectDiscData()
		{
			// Arrange

			var disc = new Disc
			{
				Id = 456,
				Title = "Beautiful Garbage",
				Year = 2001,
				Artist = new Artist
				{
					Id = 123,
					Name = "Garbage",
				},
			};

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetDisc(123, 456, It.IsAny<CancellationToken>())).ReturnsAsync(disc);

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetDisc(123, 456, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as OutputDiscData;
			Assert.IsNotNull(data);

			Assert.AreEqual("Beautiful Garbage", data.Title);
			Assert.AreEqual(2001, data.Year);
			var link = data.Links.Single();
			Assert.AreEqual("self", link.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link.Uri);
		}

		[TestMethod]
		public async Task GetDisc_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetDisc(123, 456, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetDisc(123, 456, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task CreateDisc_InvokesRepositoryCorrectly()
		{
			// Arrange

			var discData = new InputDiscData
			{
				Title = "The Fame Monster (EP)",
				Year = 2009,
			};

			var repositoryMock = new Mock<IDiscsRepository>();

			var target = new DiscsController(repositoryMock.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			await target.CreateDisc(123, discData, CancellationToken.None);

			// Assert

			Expression<Func<Disc, bool>> discDataIsValid = disc => disc.Id == 0 && disc.Title == "The Fame Monster (EP)" && disc.Year == 2009;
			repositoryMock.Verify(x => x.CreateDisc(123, It.Is(discDataIsValid), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateDisc_IfDiscWasCreated_ReturnsCreatedResult()
		{
			// Arrange

			var discData = new InputDiscData
			{
				Title = "Neo-Gothic Propaganda",
				Year = 2014,
			};

			var target = new DiscsController(Mock.Of<IDiscsRepository>(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateDisc(123, discData, CancellationToken.None);

			// Assert

			var result = actionResult as CreatedResult;
			Assert.IsNotNull(result);
			Assert.AreEqual("/SomeUri", result.Location);
		}

		[TestMethod]
		public async Task CreateDisc_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var discData = new InputDiscData
			{
				Title = "Holy Wood (In The Shadow Of The Valley Of Death)",
				Year = 2000,
			};

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.CreateDisc(123, It.IsAny<Disc>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateDisc(123, discData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task UpdateDisc_InvokesRepositoryCorrectly()
		{
			// Arrange

			var discData = new InputDiscData
			{
				Title = "Play",
				Year = 1999,
			};

			var repositoryMock = new Mock<IDiscsRepository>();

			var target = new DiscsController(repositoryMock.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			await target.UpdateDisc(123, 456, discData, CancellationToken.None);

			// Assert

			Expression<Func<Disc, bool>> discDataIsValid = disc => disc.Id == 456 && disc.Title == "Play" && disc.Year == 1999;
			repositoryMock.Verify(x => x.UpdateDisc(123, It.Is(discDataIsValid), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateDisc_IfDisctWasUpdated_ReturnsNoContentResult()
		{
			// Arrange

			var discData = new InputDiscData
			{
				Title = "Play",
				Year = 1999,
			};

			var target = new DiscsController(Mock.Of<IDiscsRepository>(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.UpdateDisc(123, 456, discData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task UpdateDisc_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var discData = new InputDiscData
			{
				Title = "Wishmaster",
				Year = 2000,
			};

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.UpdateDisc(It.IsAny<int>(), It.IsAny<Disc>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.UpdateDisc(123, 456, discData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task DeleteDisc_InvokesRepositoryCorrectly()
		{
			// Arrange

			var repositoryMock = new Mock<IDiscsRepository>();

			var target = new DiscsController(repositoryMock.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			await target.DeleteDisc(123, 456, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.DeleteDisc(123, 456, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteDisc_IfDiscWasDeleted_ReturnsNoContentResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteDisc(123, 456, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task DeleteDisc_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.DeleteDisc(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteDisc(123, 456, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}
	}
}
