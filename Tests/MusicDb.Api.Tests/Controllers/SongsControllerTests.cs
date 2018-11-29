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
using MusicDb.Api.Dto.SongDto;

namespace MusicDb.Api.Tests.Controllers
{
	[TestClass]
	public class SongsControllerTests
	{
		[TestMethod]
		public async Task GetAllDiscSongs_IfDiscWasFound_ReturnsCorrectSongsData()
		{
			// Arrange

			var disc = new Disc
			{
				Id = 456,
				Artist = new Artist
				{
					Id = 123,
				}
			};

			var songs = new[]
			{
				new Song
				{
					Id = 101,
					Title = "Velvet Darkness They Fear",
					TrackNumber = 1,
					Duration = new TimeSpan(0, 1, 4),
					Disc = disc,
				},

				new Song
				{
					Id = 102,
					Title = "Fair And 'Guiling Copesmate Death",
					TrackNumber = null,
					Duration = null,
					Disc = disc,
				},
			};

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetAllDiscSongs(123, 456, It.IsAny<CancellationToken>()))
				.ReturnsAsync(songs);

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
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

			var song1 = dataList[0];
			Assert.AreEqual("Velvet Darkness They Fear", song1.Title);
			Assert.AreEqual((short)1, song1.TrackNumber);
			Assert.AreEqual(new TimeSpan(0, 1, 4), song1.Duration);
			var link1 = song1.Links.Single();
			Assert.AreEqual("self", link1.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link1.Uri);

			var song2 = dataList[1];
			Assert.AreEqual("Fair And 'Guiling Copesmate Death", song2.Title);
			Assert.IsNull(song2.TrackNumber);
			Assert.IsNull(song2.Duration);
			var link2 = song2.Links.Single();
			Assert.AreEqual("self", link2.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link2.Uri);
		}

		[TestMethod]
		public async Task GetAllDiscSongs_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetAllDiscSongs(123, 456, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetAllDiscSongs(123, 456, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task GetSong_IfSongsWasFound_ReturnsCorrectSongData()
		{
			// Arrange

			var song = new Song
			{
				Id = 789,
				Title = "Free To Decide",
				TrackNumber = 4,
				Duration = new TimeSpan(0, 4, 24),
				Disc = new Disc
				{
					Id = 456,
					Artist = new Artist
					{
						Id = 123,
					}
				},
			};

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetSong(123, 456, 789, It.IsAny<CancellationToken>())).ReturnsAsync(song);

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.GetSong(123, 456, 789, CancellationToken.None);

			// Assert

			var result = actionResult.Result as OkObjectResult;
			Assert.IsNotNull(result);

			var data = result.Value as OutputSongData;
			Assert.IsNotNull(data);

			Assert.AreEqual("Free To Decide", data.Title);
			Assert.AreEqual((short)4, data.TrackNumber);
			Assert.AreEqual(new TimeSpan(0, 4, 24), data.Duration);
			var link = data.Links.Single();
			Assert.AreEqual("self", link.Relation);
			Assert.AreEqual(new Uri("/SomeUri", UriKind.Relative), link.Uri);
		}

		[TestMethod]
		public async Task GetSong_IfSongWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.GetSong(123, 456, 789, It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
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

			var songData = new InputSongData
			{
				Title = "Charlie Big Potato",
				TrackNumber = 1,
				Duration = new TimeSpan(0, 5, 30),
			};

			var repositoryMock = new Mock<ISongsRepository>();

			var target = new SongsController(repositoryMock.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			await target.CreateSong(123, 456, songData, CancellationToken.None);

			// Assert

			Expression<Func<Song, bool>> songDataIsValid = song => song.Id == 0 && song.Title == "Charlie Big Potato" && song.TrackNumber == 1 && song.Duration == new TimeSpan(0, 5, 30);
			repositoryMock.Verify(x => x.CreateSong(123, 456, It.Is(songDataIsValid), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task CreateSong_IfSongWasCreated_ReturnsCreatedResult()
		{
			// Arrange

			var songData = new InputSongData
			{
				Title = "Put Your Lights On",
			};

			var target = new SongsController(Mock.Of<ISongsRepository>(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateSong(123, 456, songData, CancellationToken.None);

			// Assert

			var result = actionResult as CreatedResult;
			Assert.IsNotNull(result);
			Assert.AreEqual("/SomeUri", result.Location);
		}

		[TestMethod]
		public async Task CreateSong_IfDiscWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var songData = new InputSongData
			{
				Title = "Mein Herz Brennt",
			};

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.CreateSong(123, 456, It.IsAny<Song>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.CreateSong(123, 456, songData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task UpdateSong_InvokesRepositoryCorrectly()
		{
			// Arrange

			var songData = new InputSongData
			{
				Title = "Youth Of The Nation",
				TrackNumber = 4,
				Duration = new TimeSpan(0, 4, 18),
			};

			var repositoryMock = new Mock<ISongsRepository>();

			var target = new SongsController(repositoryMock.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			await target.UpdateSong(123, 456, 789, songData, CancellationToken.None);

			// Assert

			Expression<Func<Song, bool>> songDataIsValid = song => song.Id == 789 && song.Title == "Youth Of The Nation" && song.TrackNumber == 4 && song.Duration == new TimeSpan(0, 4, 18);
			repositoryMock.Verify(x => x.UpdateSong(123, 456, It.Is(songDataIsValid), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateSong_IfSongtWasUpdated_ReturnsNoContentResult()
		{
			// Arrange

			var songData = new InputSongData
			{
				Title = "Sleeping Sun",
			};

			var target = new SongsController(Mock.Of<ISongsRepository>(), Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.UpdateSong(123, 456, 789, songData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
		}

		[TestMethod]
		public async Task UpdateSong_IfSongWasNotFound_ReturnsNotFoundResult()
		{
			// Arrange

			var songData = new InputSongData
			{
				Title = "Why Does My Heart Feel So Bad",
			};

			var repositoryStub = new Mock<ISongsRepository>();
			repositoryStub.Setup(x => x.UpdateSong(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Song>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException());

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var result = await target.UpdateSong(123, 456, 789, songData, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		}

		[TestMethod]
		public async Task DeleteSong_InvokesRepositoryCorrectly()
		{
			// Arrange

			var repositoryMock = new Mock<ISongsRepository>();

			var target = new SongsController(repositoryMock.Object, Mock.Of<ILogger<SongsController>>());
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

			var repositoryStub = new Mock<ISongsRepository>();

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
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

			var target = new SongsController(repositoryStub.Object, Mock.Of<ILogger<SongsController>>());
			target.StubControllerContext();

			// Act

			var actionResult = await target.DeleteSong(123, 456, 789, CancellationToken.None);

			// Assert

			Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
		}
	}
}
