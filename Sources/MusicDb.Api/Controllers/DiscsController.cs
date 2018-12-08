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
using MusicDb.Api.Dto.DiscDto;
using NSwag.Annotations;

namespace MusicDb.Api.Controllers
{
	[Route("api/artists/{artistId:int}")]
	public class DiscsController : ControllerBase
	{
		private readonly IDiscsRepository discsRepository;

		private readonly IMapper mapper;

		private readonly ILogger<DiscsController> logger;

		public DiscsController(IDiscsRepository discsRepository, IMapper mapper, ILogger<DiscsController> logger)
		{
			this.discsRepository = discsRepository ?? throw new ArgumentNullException(nameof(discsRepository));
			this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Retrieves all discs for specific artist.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns the list of all artist discs.
		/// </returns>
		[HttpGet("discs")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OutputDiscData>))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult<IEnumerable<OutputDiscData>>> GetAllArtistDiscs(int artistId, CancellationToken cancellationToken)
		{
			try
			{
				var discs = await discsRepository.GetAllArtistDiscs(artistId, cancellationToken).ConfigureAwait(false);

				return Ok(discs.Select(CreateDiscDto));
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to find artist {ArtistId}", artistId);
				return NotFound();
			}
		}

		/// <summary>
		/// Retrieves specific artist disc.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns disc data for requested id.
		/// </returns>
		[HttpGet("discs/{discId:int}")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(OutputDiscData))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist or disc was not found.")]
		public async Task<ActionResult<OutputDiscData>> GetDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			try
			{
				var disc = await discsRepository.GetDisc(artistId, discId, cancellationToken).ConfigureAwait(false);

				return Ok(CreateDiscDto(disc));
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to get disc {DiscId} belonging to artist {ArtistId}", discId, artistId);
				return NotFound();
			}
		}

		/// <summary>
		/// Creates new artist disc.
		/// </summary>
		/// <param name="artistId">Id of artist to which new disc belongs.</param>
		/// <param name="discDto">Data for new disc.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Returns the location of newly created disc.
		/// </returns>
		[HttpPost("discs")]
		[SwaggerResponse(HttpStatusCode.Created, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public async Task<ActionResult> CreateDisc(int artistId, [FromBody] InputDiscData discDto, CancellationToken cancellationToken)
		{
			var disc = mapper.Map<Disc>(discDto);
			int newDiscId;

			try
			{
				newDiscId = await discsRepository.CreateDisc(artistId, disc, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to create disc for artist {ArtistId}", artistId);
				return NotFound();
			}

			return Created(GetDiscUri(artistId, newDiscId), null);
		}

		/// <summary>
		/// Updates specific disc.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="discDto">New data for the disc.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Disc data was updated successfully.
		/// </returns>
		[HttpPut("discs/{discId:int}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested disc was not found.")]
		public async Task<ActionResult> UpdateDisc(int artistId, int discId, [FromBody] InputDiscData discDto, CancellationToken cancellationToken)
		{
			var disc = mapper.Map<Disc>(discDto);
			disc.Id = discId;

			try
			{
				await discsRepository.UpdateDisc(artistId, disc, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to update disc {DiscId} belonging to artist {ArtistId}", discId, artistId);
				return NotFound();
			}

			return NoContent();
		}

		/// <summary>
		/// Deletes specific disc.
		/// </summary>
		/// <param name="artistId">Artist id.</param>
		/// <param name="discId">Disc id.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// Disc was deleted successfully.
		/// </returns>
		[HttpDelete("discs/{discId:int}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested disc was not found.")]
		public async Task<ActionResult> DeleteDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			try
			{
				await discsRepository.DeleteDisc(artistId, discId, cancellationToken).ConfigureAwait(false);
			}
			catch (NotFoundException e)
			{
				logger.LogWarning(e, "Failed to delete disc {DiscId} belonging to artist {ArtistId}", discId, artistId);
				return NotFound();
			}

			return NoContent();
		}

		private OutputDiscData CreateDiscDto(Disc disc)
		{
			var discDto = mapper.Map<OutputDiscData>(disc);
			discDto.Links.Add(LinkDto.Self(GetDiscUri(disc.Artist.Id, disc.Id)));

			return discDto;
		}

		private Uri GetDiscUri(int artistId, int discId)
		{
			var values = new
			{
				artistId,
				discId
			};

			var actionUrl = Url.Action(nameof(GetDisc), null, values, Request.Scheme, Request.Host.ToUriComponent());
			return new Uri(actionUrl, UriKind.RelativeOrAbsolute);
		}
	}
}
