using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
		public async Task GetAllArtistDiscs_IfArtistWasFound_ReturnsOkResultWithDiscsData()
		{
			// Arrange

			var artist = new Artist();

			var discs = new[]
			{
				new Disc
				{
					Artist = artist,
				},

				new Disc
				{
					Artist = artist,
				},
			};

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetAllArtistDiscs(777, It.IsAny<CancellationToken>()))
				.ReturnsAsync(discs);

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
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
		}

		[TestMethod]
		public async Task GetAllArtistDiscs_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetAllArtistDiscs(777, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetAllArtistDiscs(777, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task GetDisc_IfDiscWasFound_ReturnsOkResultWithDiscData()
		{
			// Arrange

			var disc = new Disc
			{
				Artist = new Artist(),
			};

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetDisc(123, 456, It.IsAny<CancellationToken>())).ReturnsAsync(disc);

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetDisc(123, 456, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as OutputDiscData;
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public async Task GetDisc_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.GetDisc(123, 456, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
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

			var discData = new InputDiscData();
			var disc = new Disc();

			var repositoryMock = new Mock<IDiscsRepository>();

			var mapperStub = new Mock<IMapper>();
			mapperStub.Setup(x => x.Map<Disc>(discData)).Returns(disc);

			var target = new DiscsController(repositoryMock.Object, mapperStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			await target.CreateDisc(123, discData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.CreateDisc(123, It.Is<Disc>(d => Object.ReferenceEquals(d, disc) && d.Id == 0), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateDisc_IfDiscWasCreated_ReturnsCreatedResult()
		{
			// Arrange

			var target = new DiscsController(Mock.Of<IDiscsRepository>(), StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateDisc(123, new InputDiscData(), CancellationToken.None);

			// Assert

			var result = actionResult as CreatedResult;
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public async Task CreateDisc_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.CreateDisc(123, It.IsAny<Disc>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateDisc(123, new InputDiscData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task UpdateDisc_InvokesRepositoryCorrectly()
		{
			// Arrange

			var discData = new InputDiscData();
			var disc = new Disc();

			var repositoryMock = new Mock<IDiscsRepository>();

			var mapperStub = new Mock<IMapper>();
			mapperStub.Setup(x => x.Map<Disc>(discData)).Returns(disc);

			var target = new DiscsController(repositoryMock.Object, mapperStub.Object, Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			await target.UpdateDisc(123, 456, discData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.UpdateDisc(123, It.Is<Disc>(d => Object.ReferenceEquals(d, disc) && d.Id == 456), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateDisc_IfDisctWasUpdated_ReturnsNoContentResult()
		{
			// Arrange

			var target = new DiscsController(Mock.Of<IDiscsRepository>(), StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.UpdateDisc(123, 456, new InputDiscData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task UpdateDisc_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IDiscsRepository>();
			repositoryStub.Setup(x => x.UpdateDisc(It.IsAny<int>(), It.IsAny<Disc>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.UpdateDisc(123, 456, new InputDiscData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task DeleteDisc_InvokesRepositoryCorrectly()
		{
			// Arrange

			var repositoryMock = new Mock<IDiscsRepository>();

			var target = new DiscsController(repositoryMock.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
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

			var target = new DiscsController(Mock.Of<IDiscsRepository>(), StubMapper(), Mock.Of<ILogger<DiscsController>>());
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

			var target = new DiscsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<DiscsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteDisc(123, 456, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}

		private static IMapper StubMapper()
		{
			var mapperStub = new Mock<IMapper>();

			mapperStub.Setup(x => x.Map<Disc>(It.IsAny<InputDiscData>()))
				.Returns(new Disc());

			mapperStub.Setup(x => x.Map<OutputDiscData>(It.IsAny<Disc>()))
				.Returns(new OutputDiscData());

			return mapperStub.Object;
		}
	}
}
