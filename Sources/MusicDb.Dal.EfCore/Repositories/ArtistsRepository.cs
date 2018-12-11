using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.EfCore.Internal;
using Npgsql;

namespace MusicDb.Dal.EfCore.Repositories
{
	public class ArtistsRepository : IArtistsRepository
	{
		private readonly MusicDbContext context;

		private readonly IEntityLocator entityLocator;

		public ArtistsRepository(MusicDbContext context, IEntityLocator entityLocator)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			this.entityLocator = entityLocator ?? throw new ArgumentNullException(nameof(entityLocator));
		}

		public async Task<int> CreateArtist(Artist artist, CancellationToken cancellationToken)
		{
			context.Artists.Add(artist);

			await SaveChanges(cancellationToken).ConfigureAwait(false);

			return artist.Id;
		}

		public async Task<ICollection<Artist>> GetAllArtists(CancellationToken cancellationToken)
		{
			return await context.Artists
				.OrderBy(a => a.Id)
				.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		public Task<Artist> GetArtist(int artistId, CancellationToken cancellationToken)
		{
			return entityLocator.FindArtist(artistId, false, cancellationToken);
		}

		public async Task UpdateArtist(Artist artist, CancellationToken cancellationToken)
		{
			var artistEntity = await entityLocator.FindArtist(artist.Id, false, cancellationToken).ConfigureAwait(false);
			context.Entry(artistEntity).CurrentValues.SetValues(artist);

			await SaveChanges(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteArtist(int artistId, CancellationToken cancellationToken)
		{
			var artistEntity = await entityLocator.FindArtist(artistId, false, cancellationToken).ConfigureAwait(false);
			context.Artists.Remove(artistEntity);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private async Task SaveChanges(CancellationToken cancellationToken)
		{
			try
			{
				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (DbUpdateException e) when (e.InnerException is PostgresException pgException && pgException.SqlState == PostgresErrors.UniqueViolation)
			{
				throw new DuplicateKeyException("Failed to save changes to the database", e);
			}
		}
	}
}
