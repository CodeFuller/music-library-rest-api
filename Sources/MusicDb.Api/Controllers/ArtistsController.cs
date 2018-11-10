using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using MusicDb.Api.Dto;

namespace MusicDb.Api.Controllers
{
	/// <summary>
	/// Artist resource represents solo artist or a band.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class ArtistsController : ControllerBase
	{
		/// <summary>
		/// Retrieves all database artists.
		/// </summary>
		/// <returns>
		/// Returns the list of all database artists.
		/// </returns>
		[HttpGet]
		[SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<ArtistDto>))]
		public ActionResult<IEnumerable<ArtistDto>> Get()
		{
			return Array.Empty<ArtistDto>();
		}

		/// <summary>
		/// Retrieves specific artist.
		/// </summary>
		/// <param name="id">Id of requested artist.</param>
		/// <returns>
		/// Returns artist data for requested id.
		/// </returns>
		[HttpGet("{id}")]
		[SwaggerResponse(HttpStatusCode.OK, typeof(ArtistDto))]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public ActionResult<ArtistDto> Get(int id)
		{
			return NotFound();
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
		public ActionResult Post([FromBody] ArtistDto artist)
		{
			return CreatedAtAction(nameof(Get), new { id = 1 }, null);
		}

		/// <summary>
		/// Updates specific artist.
		/// </summary>
		/// <param name="id">Id of updated artist.</param>
		/// <param name="artist">New data for the artist.</param>
		/// <returns>
		/// Artist data was updated successfully.
		/// </returns>
		[HttpPut("{id}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public ActionResult Put(int id, [FromBody] ArtistDto artist)
		{
			return NoContent();
		}

		/// <summary>
		/// Deletes specific artist.
		/// </summary>
		/// <param name="id">Id of deleted artist.</param>
		/// <returns>
		/// Artist was deleted successfully.
		/// </returns>
		[HttpDelete("{id}")]
		[SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
		[SwaggerResponse(HttpStatusCode.Forbidden, typeof(void), Description = "Principal is not authorized for database modification.")]
		[SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Requested artist was not found.")]
		public ActionResult Delete(int id)
		{
			return NoContent();
		}
	}
}
