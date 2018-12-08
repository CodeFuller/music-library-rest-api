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
using MusicDb.Api.Dto.ArtistDto;

namespace MusicDb.Api.Tests.Controllers
{
	[TestClass]
	public class ArtistsControllerTests
	{
		[TestMethod]
		public async Task GetArtists_ReturnsOkResultWithArtistsData()
		{
			// Arrange

			var artists = new[]
			{
				new Artist(),
				new Artist(),
			};

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.GetAllArtists(It.IsAny<CancellationToken>()))
				.ReturnsAsync(artists);

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetArtists(CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as IEnumerable<OutputArtistData>;
			Assert.IsNotNull(data);
			var dataList = data.ToList();
			Assert.AreEqual(2, dataList.Count);
		}

		[TestMethod]
		public async Task GetArtist_IfArtistWasFound_ReturnsOkResultWithArtistData()
		{
			// Arrange

			var artist = new Artist();

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.GetArtist(123, It.IsAny<CancellationToken>())).ReturnsAsync(artist);

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetArtist(123, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as OutputArtistData;
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public async Task GetArtist_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.GetArtist(123, It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.GetArtist(123, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task CreateArtist_InvokesRepositoryCorrectly()
		{
			// Arrange

			var artistData = new InputArtistData();
			var artist = new Artist();

			var repositoryMock = new Mock<IArtistsRepository>();

			var mapperStub = new Mock<IMapper>();
			mapperStub.Setup(x => x.Map<Artist>(artistData)).Returns(artist);

			var target = new ArtistsController(repositoryMock.Object, mapperStub.Object, Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			await target.CreateArtist(artistData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.CreateArtist(It.Is<Artist>(a => Object.ReferenceEquals(a, artist) && a.Id == 0), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateArtist_IfArtistWasCreated_ReturnsCreatedResult()
		{
			// Arrange

			var target = new ArtistsController(Mock.Of<IArtistsRepository>(), StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateArtist(new InputArtistData(), CancellationToken.None);

			// Assert

			var result = actionResult as CreatedResult;
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public async Task CreateArtist_IfArtistWithSuchNameAlreadyExists_ReturnsConflictResult()
		{
			// Arrange

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.CreateArtist(It.IsAny<Artist>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateKeyException());

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateArtist(new InputArtistData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(ConflictResult));
		}

		[TestMethod]
		public async Task UpdateArtist_InvokesRepositoryCorrectly()
		{
			// Arrange

			var artistData = new InputArtistData();
			var artist = new Artist();

			var repositoryMock = new Mock<IArtistsRepository>();

			var mapperStub = new Mock<IMapper>();
			mapperStub.Setup(x => x.Map<Artist>(artistData)).Returns(artist);

			var target = new ArtistsController(repositoryMock.Object, mapperStub.Object, Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			await target.UpdateArtist(123, artistData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.UpdateArtist(It.Is<Artist>(a => Object.ReferenceEquals(a, artist) && a.Id == 123), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistWasUpdated_ReturnsNoContentResult()
		{
			// Arrange

			var target = new ArtistsController(Mock.Of<IArtistsRepository>(), StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.UpdateArtist(123, new InputArtistData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.UpdateArtist(It.IsAny<Artist>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.UpdateArtist(123, new InputArtistData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistWithSuchNameAlreadyExists_ReturnsConflictdResult()
		{
			// Arrange

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.UpdateArtist(It.IsAny<Artist>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateKeyException());

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.UpdateArtist(123, new InputArtistData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(ConflictResult));
		}

		[TestMethod]
		public async Task DeleteArtist_InvokesRepositoryCorrectly()
		{
			// Arrange

			var repositoryMock = new Mock<IArtistsRepository>();

			var target = new ArtistsController(repositoryMock.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			await target.DeleteArtist(123, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.DeleteArtist(123, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteArtist_IfArtistWasDeleted_ReturnsNoContentResult()
		{
			// Arrange

			var target = new ArtistsController(Mock.Of<IArtistsRepository>(), StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteArtist(123, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task DeleteArtist_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.DeleteArtist(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new ArtistsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<ArtistsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.DeleteArtist(123, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		private static IMapper StubMapper()
		{
			var mapperStub = new Mock<IMapper>();

			mapperStub.Setup(x => x.Map<Artist>(It.IsAny<InputArtistData>()))
				.Returns(new Artist());

			mapperStub.Setup(x => x.Map<OutputArtistData>(It.IsAny<Artist>()))
				.Returns(new OutputArtistData());

			return mapperStub.Object;
		}
	}
}
