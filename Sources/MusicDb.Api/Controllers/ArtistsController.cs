using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;
using MusicDb.Api.Dto;
using NSwag.Annotations;

namespace MusicDb.Api.Controllers
{
	/// <summary>
	/// Artist resource represents solo artist or a band.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class ArtistsController : ControllerBase
	{
		private readonly IArtistsRepository repository;

		private readonly ILogger<ArtistsController> logger;

		public ArtistsController(IArtistsRepository repository, ILogger<ArtistsController> logger)
		{
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
		/// <param name="id">Id of requested artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns artist data for requested id.
		/// </returns>
		[HttpGet("{id}")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(OutputArtistData))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult<OutputArtistData>> GetArtist(int id, CancellationToken cancellationToken)
		{
			try
			{
				var artist = await repository.GetArtist(id, cancellationToken).ConfigureAwait(false);
				return CreateArtistDto(artist);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, $"Failed to find artist with id of {id}");
				return NotFound();
			}
		}

		/// <summary>
		/// Creates new artist.
		/// </summary>
		/// <returns>
		/// Returns the location of newly created artist.
		/// </returns>
		[HttpPost]
		[SwaggerResponse(HttpStatusCode.Created, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.Conflict, typeof(void), Description = "The artist with specified name already exists.")]
		public async Task<ActionResult> Post([FromBody] InputArtistData artist, CancellationToken cancellationToken)
		{
			var model = artist.ToModel();

			try
			{
				await repository.CreateArtist(model, cancellationToken).ConfigureAwait(false);
			}
			catch (DuplicateKeyException e)
			{
				logger.LogWarning(e, $"Failed to create artist '{model.Name}'");
				return Conflict();
			}

			return Created(GetArtistUri(model.Id), null);
		}

		/// <summary>
		/// Updates specific artist.
		/// </summary>
		/// <param name="id">Id of updated artist.</param>
		/// <param name="artist">New data for the artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Artist data was updated successfully.
		/// </returns>
		[HttpPut("{id}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult> Put(int id, [FromBody] InputArtistData artist, CancellationToken cancellationToken)
		{
			var model = artist.ToModel(id);

			try
			{
				await repository.UpdateArtist(model, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, $"Failed to find artist with id of {id}");
				return NotFound();
			}
			catch (DuplicateKeyException e)
			{
				logger.LogWarning(e, $"Failed to update artist with id of {id}");
				return Conflict();
			}

			return NoContent();
		}

		/// <summary>
		/// Deletes specific artist.
		/// </summary>
		/// <param name="id">Id of deleted artist.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Artist was deleted successfully.
		/// </returns>
		[HttpDelete("{id}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
		{
			try
			{
				await repository.DeleteArtist(id, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, $"Failed to find artist with id of {id}");
				return NotFound();
			}

			return NoContent();
		}

		private OutputArtistData CreateArtistDto(Artist artist)
		{
			return new OutputArtistData(artist, GetArtistUri(artist.Id));
		}

		private Uri GetArtistUri(int artistId)
		{
			var actionUrl = Url.Action(nameof(GetArtist), null, new { id = artistId }, Request.Scheme, Request.Host.ToUriComponent());
			return new Uri(actionUrl, UriKind.RelativeOrAbsolute);
		}
	}
}
