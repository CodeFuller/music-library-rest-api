using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.SqlServer.Repositories
{
	public abstract class BasicRepository
	{
		protected MusicDbContext Context { get; }

		protected BasicRepository(MusicDbContext context)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected async Task<Artist> FindArtist(int artistId, bool includeDiscs, CancellationToken cancellationToken)
		{
			var artistsQueryable = Context.Artists.AsQueryable();
			if (includeDiscs)
			{
				artistsQueryable = artistsQueryable.Include(a => a.Discs);
			}

			var artistEntity = await artistsQueryable
				.Where(a => a.Id == artistId)
				.FirstOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (artistEntity == null)
			{
				throw new NotFoundException($"Artist with id of {artistId} was not found");
			}

			return artistEntity;
		}
	}
}
