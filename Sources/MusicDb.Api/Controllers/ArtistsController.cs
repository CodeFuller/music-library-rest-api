﻿using System;
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
using MusicDb.Api.Dto.ArtistDto;
using NSwag.Annotations;

namespace MusicDb.Api.Controllers
{
	/// <summary>
	/// Artist resource represents solo artist or a band.
	/// </summary>
	[Route("api/artists")]
	[ApiController]
	public class ArtistsController : ControllerBase
	{
		private readonly IArtistsRepository repository;

		private readonly IMapper mapper;

		private readonly ILogger<ArtistsController> logger;

		public ArtistsController(IArtistsRepository repository, IMapper mapper, ILogger<ArtistsController> logger)
		{
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
			this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Retrieves all database artists.
		/// </summary>
		/// <returns>
		/// Returns the list of all database artists.
		/// </returns>
		[HttpGet]
		[SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OutputArtistData>))]
		public async Task<ActionResult<IEnumerable<OutputArtistData>>> GetArtists(CancellationToken cancellationToken)
		{
			var artists = await repository.GetAllArtists(cancellationToken).ConfigureAwait(false);

			return Ok(artists.Select(CreateArtistDto));
		}

		/// <summary>
		/// Retrieves specific artist.
		/// </summary>
		/// <param name="artistId">Id of requested artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns artist data for requested id.
		/// </returns>
		[HttpGet("{artistId:int}")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(OutputArtistData))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult<OutputArtistData>> GetArtist(int artistId, CancellationToken cancellationToken)
		{
			try
			{
				var artist = await repository.GetArtist(artistId, cancellationToken).ConfigureAwait(false);
				return Ok(CreateArtistDto(artist));
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to find artist {ArtistId}", artistId);
				return NotFound();
			}
		}

		/// <summary>
		/// Creates new artist.
		/// </summary>
		/// <param name="artistDto">Data for new artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns the location of newly created artist.
		/// </returns>
		[HttpPost]
		[SwaggerResponse(HttpStatusCode.Created, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.Conflict, typeof(void), Description = "The artist with specified name already exists.")]
		public async Task<ActionResult> CreateArtist([FromBody] InputArtistData artistDto, CancellationToken cancellationToken)
		{
			var artist = mapper.Map<Artist>(artistDto);

			int newArtistId;

			try
			{
				newArtistId = await repository.CreateArtist(artist, cancellationToken).ConfigureAwait(false);
			}
			catch (DuplicateKeyException e)
			{
				logger.LogWarning(e, "Failed to create artist '{ArtistName}'", artist.Name);
				return Conflict();
			}

			return Created(GetArtistUri(newArtistId), null);
		}

		/// <summary>
		/// Updates specific artist.
		/// </summary>
		/// <param name="artistId">Id of updated artist.</param>
		/// <param name="artistDto">New data for the artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Artist data was updated successfully.
		/// </returns>
		[HttpPut("{artistId:int}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult> UpdateArtist(int artistId, [FromBody] InputArtistData artistDto, CancellationToken cancellationToken)
		{
			var artist = mapper.Map<Artist>(artistDto);
			artist.Id = artistId;

			try
			{
				await repository.UpdateArtist(artist, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to find artist {ArtistId}", artistId);
				return NotFound();
			}
			catch (DuplicateKeyException e)
			{
				logger.LogWarning(e, "Failed to update artist {ArtistId}", artistId);
				return Conflict();
			}

			return NoContent();
		}

		/// <summary>
		/// Deletes specific artist.
		/// </summary>
		/// <param name="artistId">Id of deleted artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Artist was deleted successfully.
		/// </returns>
		[HttpDelete("{artistId:int}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult> DeleteArtist(int artistId, CancellationToken cancellationToken)
		{
			try
			{
				await repository.DeleteArtist(artistId, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to delete artist {ArtistId}", artistId);
				return NotFound();
			}

			return NoContent();
		}

		private OutputArtistData CreateArtistDto(Artist artist)
		{
			var artistDto = mapper.Map<OutputArtistData>(artist);
			artistDto.Links.Add(LinkDto.Self(GetArtistUri(artist.Id)));

			return artistDto;
		}

		private Uri GetArtistUri(int artistId)
		{
			var actionUrl = Url.Action(nameof(GetArtist), null, new { artistId }, Request.Scheme, Request.Host.ToUriComponent());
			return new Uri(actionUrl, UriKind.RelativeOrAbsolute);
		}
	}
}
