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
using MusicDb.Api.Dto.SongDto;

namespace MusicDb.Api.Tests.Controllers
{
	[TestClass]
	public class SongsControllerTests
	{
		[TestMethod]
		public async Task GetAllDiscSongs_IfDiscWasFound_ReturnsOkResultWithSongsData()
		{
			// Arrange

			var disc = new Disc
			{
				Artist = new Artist()
			};

			var songs = new[]
			{
				new Song
				{
					Disc = disc,
				},

				new Song
				{
					Disc = disc,
				},
			};

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetAllDiscSongs(123, 456, It.IsAny<CancellationToken>()))
				.ReturnsAsync(songs);

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetAllDiscSongs(123, 456, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as IEnumerable<OutputSongData>;
			Assert.IsNotNull(data);
			var dataList = data.ToList();
			Assert.AreEqual(2, dataList.Count);
		}

		[TestMethod]
		public async Task GetAllDiscSongs_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetAllDiscSongs(123, 456, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetAllDiscSongs(123, 456, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task GetSong_IfSongsWasFound_ReturnsOkResultWithSongData()
		{
			// Arrange

			var song = new Song
			{
				Disc = new Disc
				{
					Artist = new Artist()
				},
			};

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetSong(123, 456, 789, It.IsAny<CancellationToken>())).ReturnsAsync(song);

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetSong(123, 456, 789, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as OutputSongData;
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public async Task GetSong_IfSongWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetSong(123, 456, 789, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetSong(123, 456, 789, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task CreateSong_InvokesRepositoryCorrectly()
		{
			// Arrange

			var songData = new InputSongData();
			var song = new Song();

			var repositoryMock = new Mock<ISongsRepository>();

			var mapperStub = new Mock<IMapper>();
			mapperStub.Setup(x => x.Map<Song>(songData)).Returns(song);

			var target = new SongsController(repositoryMock.Object, mapperStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			await target.CreateSong(123, 456, songData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.CreateSong(123, 456, It.Is<Song>(s => Object.ReferenceEquals(s, song) && s.Id == 0), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateSong_IfSongWasCreated_ReturnsCreatedResult()
		{
			// Arrange

			var target = new SongsController(Mock.Of<ISongsRepository>(), StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateSong(123, 456, new InputSongData(), CancellationToken.None);

			// Assert

			var result = actionResult as CreatedResult;
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public async Task CreateSong_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.CreateSong(123, 456, It.IsAny<Song>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateSong(123, 456, new InputSongData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task UpdateSong_InvokesRepositoryCorrectly()
		{
			// Arrange

			var songData = new InputSongData();
			var song = new Song();

			var repositoryMock = new Mock<ISongsRepository>();

			var mapperStub = new Mock<IMapper>();
			mapperStub.Setup(x => x.Map<Song>(songData)).Returns(song);

			var target = new SongsController(repositoryMock.Object, mapperStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			await target.UpdateSong(123, 456, 789, songData, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.UpdateSong(123, 456, It.Is<Song>(s => Object.ReferenceEquals(s, song) && s.Id == 789), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateSong_IfSongtWasUpdated_ReturnsNoContentResult()
		{
			// Arrange

			var target = new SongsController(Mock.Of<ISongsRepository>(), StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.UpdateSong(123, 456, 789, new InputSongData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task UpdateSong_IfSongWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.UpdateSong(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Song>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.UpdateSong(123, 456, 789, new InputSongData(), CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task DeleteSong_InvokesRepositoryCorrectly()
		{
			// Arrange

			var repositoryMock = new Mock<ISongsRepository>();

			var target = new SongsController(repositoryMock.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			await target.DeleteSong(123, 456, 789, CancellationToken.None);

			// Assert

			repositoryMock.Verify(x => x.DeleteSong(123, 456, 789, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteSong_IfSongWasDeleted_ReturnsNoContentResult()
		{
			// Arrange

			var target = new SongsController(Mock.Of<ISongsRepository>(), StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteSong(123, 456, 789, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task DeleteSong_IfSongWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.DeleteSong(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, StubMapper(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteSong(123, 456, 789, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}

		private static IMapper StubMapper()
		{
			var mapperStub = new Mock<IMapper>();

			mapperStub.Setup(x => x.Map<Song>(It.IsAny<InputSongData>()))
				.Returns(new Song());

			mapperStub.Setup(x => x.Map<OutputSongData>(It.IsAny<Song>()))
				.Returns(new OutputSongData());

			return mapperStub.Object;
		}
	}
}
