using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
		public async Task GetArtists_ReturnsCorrectArtistsData()
		{
			// Arrange

			var artists = new[]
			{
				new Artist
				{
					Name = "Epica",
				},

				new Artist
				{
					Name = "Within Temptation",
				},
			};

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.GetAllArtists(It.IsAny<CancellationToken>()))
				.ReturnsAsync(artists);

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var actionResult = await target.GetArtists(CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as IEnumerable<OutputArtistData>;
			Assert.IsNotNull(data);
			var dataList = data.ToList();
			Assert.AreEqual(2, dataList.Count);

			var artistDto1 = dataList[0];
			Assert.AreEqual("Epica", artistDto1.Name);
			var link1 = artistDto1.Links.Single();
			Assert.AreEqual("self", link1.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link1.Uri);

			var artistDto2 = dataList[1];
			Assert.AreEqual("Within Temptation", artistDto2.Name);
			var link2 = artistDto1.Links.Single();
			Assert.AreEqual("self", link2.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link2.Uri);
		}

		[TestMethod]
		public async Task GetArtist_IfArtistWasFound_ReturnsCorrectArtistData()
		{
			// Arrange

			var artist = new Artist
			{
				Name = "Imperia",
			};

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.GetArtist(123, It.IsAny<CancellationToken>())).ReturnsAsync(artist);

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var actionResult = await target.GetArtist(123, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as OutputArtistData;
			Assert.IsNotNull(data);

			Assert.AreEqual("Imperia", data.Name);
			var link = data.Links.Single();
			Assert.AreEqual("self", link.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link.Uri);
		}

		[TestMethod]
		public async Task GetArtist_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.GetArtist(123, It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var result = await target.GetArtist(123, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task CreateArtist_InvokesRepositoryCorrectly()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "Nightwish",
			};

			var repositoryMock = new Mock<IArtistsRepository>();

			var target = new ArtistsController(repositoryMock.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			await target.CreateArtist(artistData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.CreateArtist(It.Is<Artist>(a => a.Id == 0 && a.Name == "Nightwish"), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateArtist_IfArtistWasCreated_ReturnsCreatedResult()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "Linkin Park",
			};

			var target = new ArtistsController(Mock.Of<IArtistsRepository>(), Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var actionResult = await target.CreateArtist(artistData, CancellationToken.None);

			// Assert

			var result = actionResult as CreatedResult;
			Assert.IsNotNull(result);
			Assert.AreEqual("/SomeUri", result.Location);
		}

		[TestMethod]
		public async Task CreateArtist_IfArtistWithSuchNameAlreadyExists_ReturnsConflictResult()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "Limp Bizkit",
			};

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.CreateArtist(It.IsAny<Artist>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateKeyException());

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var actionResult = await target.CreateArtist(artistData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(ConflictResult));
		}

		[TestMethod]
		public async Task UpdateArtist_InvokesRepositoryCorrectly()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "AC/DC",
			};

			var repositoryMock = new Mock<IArtistsRepository>();

			var target = new ArtistsController(repositoryMock.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			await target.UpdateArtist(123, artistData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.UpdateArtist(It.Is<Artist>(a => a.Id == 123 && a.Name == "AC/DC"), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistWasUpdated_ReturnsNoContentResult()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "Depeche Mode",
			};

			var target = new ArtistsController(Mock.Of<IArtistsRepository>(), Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var actionResult = await target.UpdateArtist(123, artistData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "Evanescence",
			};

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.UpdateArtist(It.IsAny<Artist>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var result = await target.UpdateArtist(123, artistData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task UpdateArtist_IfArtistWithSuchNameAlreadyExists_ReturnsConflictdResult()
		{
			// Arrange

			var artistData = new InputArtistData
			{
				Name = "Evanescence",
			};

			var repositoryStub = new Mock<IArtistsRepository>();
			repositoryStub.Setup(x => x.UpdateArtist(It.IsAny<Artist>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateKeyException());

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var result = await target.UpdateArtist(123, artistData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(ConflictResult));
		}

		[TestMethod]
		public async Task DeleteArtist_InvokesRepositoryCorrectly()
		{
			// Arrange

			var repositoryMock = new Mock<IArtistsRepository>();

			var target = new ArtistsController(repositoryMock.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			await target.DeleteArtist(123, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.DeleteArtist(123, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteArtist_IfArtistWasDeleted_ReturnsNoContentResult()
		{
			// Arrange

			var target = new ArtistsController(Mock.Of<IArtistsRepository>(), Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

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

			var target = new ArtistsController(repositoryStub.Object, Mock.Of<ILogger<ArtistsController>>());
			SetupControllerContext(target);

			// Act

			var result = await target.DeleteArtist(123, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		private static void SetupControllerContext(ArtistsController controller)
		{
			var httpContextStub = new Mock<HttpContext>();
			httpContextStub.Setup(x => x.Request).Returns(Mock.Of<HttpRequest>());

			controller.ControllerContext.HttpContext = httpContextStub.Object;

			var urlHelperStub = new Mock<IUrlHelper>();
			urlHelperStub.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/SomeUri");
			controller.Url = urlHelperStub.Object;
		}
	}
}
