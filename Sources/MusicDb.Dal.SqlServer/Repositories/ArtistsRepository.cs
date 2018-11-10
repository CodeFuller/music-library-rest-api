using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer.Internal;

namespace MusicDb.Dal.SqlServer.Repositories
{
	public class ArtistsRepository : IArtistsRepository
	{
		private readonly MusicDbContext context;

		public ArtistsRepository(MusicDbContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task CreateArtist(Artist artist, CancellationToken cancellationToken)
		{
			context.Artists.Add(artist);

			await SaveChanges(cancellationToken).ConfigureAwait(false);
		}

		public async Task<ICollection<Artist>> GetAllArtists(CancellationToken cancellationToken)
		{
			return await context.Artists
				.OrderBy(a => a.Id)
				.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task<Artist> GetArtist(int artistId, CancellationToken cancellationToken)
		{
			return await FindArtist(artistId, cancellationToken).ConfigureAwait(false);
		}

		public async Task UpdateArtist(Artist artist, CancellationToken cancellationToken)
		{
			var artistEntity = await FindArtist(artist.Id, cancellationToken).ConfigureAwait(false);
			context.Entry(artistEntity).CurrentValues.SetValues(artist);

			await SaveChanges(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteArtist(int artistId, CancellationToken cancellationToken)
		{
			var artistEntity = await FindArtist(artistId, cancellationToken).ConfigureAwait(false);
			context.Artists.Remove(artistEntity);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private async Task<Artist> FindArtist(int artistId, CancellationToken cancellationToken)
		{
			var artistEntity = await context.Artists
				.Where(a => a.Id == artistId)
				.FirstOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (artistEntity == null)
			{
				throw new NotFoundException($"Artist with id of {artistId} was not found");
			}

			return artistEntity;
		}

		private async Task SaveChanges(CancellationToken cancellationToken)
		{
			try
			{
				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (DbUpdateException e) when (e.InnerException is SqlException sqlException && sqlException.Number == SqlErrors.DuplicateKey)
			{
				throw new DuplicateKeyException("Failed to save changes to the database", e);
			}
		}
	}
}
