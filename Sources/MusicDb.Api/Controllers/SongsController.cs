using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;
using MusicDb.Api.Dto;
using MusicDb.Api.Dto.SongDto;
using NSwag.Annotations;

namespace MusicDb.Api.Controllers
{
	[Route("api/artists/{artistId:int}/discs/{discId:int}")]
	public class SongsController : ControllerBase
	{
		private readonly ISongsRepository songsRepository;

		private readonly IMapper mapper;

		private readonly ILogger<SongsController> logger;

		public SongsController(ISongsRepository songsRepository, IMapper mapper, ILogger<SongsController> logger)
		{
			this.songsRepository = songsRepository ?? throw new ArgumentNullException(nameof(songsRepository));
			this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Retrieves all disc songs.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns the list of all disc songs.
		/// </returns>
		[HttpGet("songs")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OutputSongData>))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist or disc was not found.")]
		public async Task<ActionResult<IEnumerable<OutputSongData>>> GetAllDiscSongs(int artistId, int discId, CancellationToken cancellationToken)
		{
			try
			{
				var songs = await songsRepository.GetAllDiscSongs(artistId, discId, cancellationToken).ConfigureAwait(false);

				return Ok(songs.Select(CreateSongDto));
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to find artist {ArtistId}", artistId);
				return NotFound();
			}
		}

		/// <summary>
		/// Retrieves specific song content.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="songId">Song id.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns content of requested song.
		/// </returns>
		[HttpGet("songs/{songId:int}")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(string))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist or disc was not found.")]
		public async Task<ActionResult<string>> GetSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			try
			{
				var song = await songsRepository.GetSong(artistId, discId, songId, cancellationToken).ConfigureAwait(false);

				return Ok(CreateSongDto(song));
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to find song {SongId} from disc {DiscId} of artist {ArtistId}", songId, discId, artistId);
				return NotFound();
			}
		}

		/// <summary>
		/// Creates new disc song.
		/// </summary>
		/// <param name="artistId">Id of artist to which the disc belongs.</param>
		/// <param name="discId">Id of disc to which new song belongs.</param>
		/// <param name="songDto">Data for new song.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns the location of newly created song.
		/// </returns>
		[HttpPost("songs")]
		[SwaggerResponse(HttpStatusCode.Created, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist or disc was not found.")]
		public async Task<ActionResult> CreateSong(int artistId, int discId, [FromBody] InputSongData songDto, CancellationToken cancellationToken)
		{
			var song = mapper.Map<Song>(songDto);
			int newSongId;

			try
			{
				newSongId = await songsRepository.CreateSong(artistId, discId, song, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to create song for disc {DiscId} of artist {ArtistId}", discId, artistId);
				return NotFound();
			}

			return Created(GetSongUri(artistId, discId, newSongId), null);
		}

		/// <summary>
		/// Updates specific song.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="songId">Song id.</param>
		/// <param name="songDto">New data for the song.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Song data was updated successfully.
		/// </returns>
		[HttpPut("songs/{songId:int}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested song was not found.")]
		public async Task<ActionResult> UpdateSong(int artistId, int discId, int songId, [FromBody] InputSongData songDto, CancellationToken cancellationToken)
		{
			var song = mapper.Map<Song>(songDto);
			song.Id = songId;

			try
			{
				await songsRepository.UpdateSong(artistId, discId, song, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to update song {SongId} from disc {DiscId} of artist {ArtistId}", songId, discId, artistId);
				return NotFound();
			}

			return NoContent();
		}

		/// <summary>
		/// Deletes specific song.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="songId">Song id.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Song was deleted successfully.
		/// </returns>
		[HttpDelete("songs/{songId:int}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested song was not found.")]
		public async Task<ActionResult> DeleteSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			try
			{
				await songsRepository.DeleteSong(artistId, discId, songId, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to delete song {SongId} from disc {DiscId} of artist {ArtistId}", songId, discId, artistId);
				return NotFound();
			}

			return NoContent();
		}

		private OutputSongData CreateSongDto(Song song)
		{
			var disc = song.Disc;
			var songDto = mapper.Map<OutputSongData>(song);
			songDto.Links.Add(LinkDto.Self(GetSongUri(disc.Artist.Id, disc.Id, song.Id)));

			return songDto;
		}

		private Uri GetSongUri(int artistId, int discId, int songId)
		{
			var values = new
			{
				artistId,
				discId,
				songId,
			};

			var actionUrl = Url.Action(nameof(GetSong), null, values, Request.Scheme, Request.Host.ToUriComponent());
			return new Uri(actionUrl, UriKind.RelativeOrAbsolute);
		}
	}
}
