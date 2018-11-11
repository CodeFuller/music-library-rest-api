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
	public class ArtistsRepository : BasicRepository, IArtistsRepository
	{
		public ArtistsRepository(MusicDbContext context)
			: base(context)
		{
		}

		public async Task CreateArtist(Artist artist, CancellationToken cancellationToken)
		{
			Context.Artists.Add(artist);

			await SaveChanges(cancellationToken).ConfigureAwait(false);
		}

		public async Task<ICollection<Artist>> GetAllArtists(CancellationToken cancellationToken)
		{
			return await Context.Artists
				.OrderBy(a => a.Id)
				.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task<Artist> GetArtist(int artistId, CancellationToken cancellationToken)
		{
			return await FindArtist(artistId, includeDiscs: false, cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		public async Task UpdateArtist(Artist artist, CancellationToken cancellationToken)
		{
			var artistEntity = await FindArtist(artist.Id, includeDiscs: false, cancellationToken: cancellationToken).ConfigureAwait(false);
			Context.Entry(artistEntity).CurrentValues.SetValues(artist);

			await SaveChanges(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteArtist(int artistId, CancellationToken cancellationToken)
		{
			var artistEntity = await FindArtist(artistId, includeDiscs: false, cancellationToken: cancellationToken).ConfigureAwait(false);
			Context.Artists.Remove(artistEntity);

			await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private async Task SaveChanges(CancellationToken cancellationToken)
		{
			try
			{
				await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (DbUpdateException e) when (e.InnerException is SqlException sqlException && sqlException.Number == SqlErrors.DuplicateKey)
			{
				throw new DuplicateKeyException("Failed to save changes to the database", e);
			}
		}
	}
}
