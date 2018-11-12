using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.SqlServer.Internal
{
	internal static class QueryableExtensions
	{
		public static async Task<Artist> FindArtist(this IQueryable<Artist> artists, int artistId, CancellationToken cancellationToken)
		{
			var artistEntity = await artists
				.Where(a => a.Id == artistId)
				.SingleOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (artistEntity == null)
			{
				throw new NotFoundException($"Artist {artistId} was not found");
			}

			return artistEntity;
		}

		public static async Task<Disc> FindDisc(this IQueryable<Disc> discs, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await discs
				.Where(d => d.Id == discId)
				.SingleOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (discEntity == null)
			{
				throw new NotFoundException($"Disc {discId} was not found");
			}

			return discEntity;
		}

		public static async Task<Disc> FindArtistDisc(this IQueryable<Disc> discs, int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await discs
				.FindDisc(discId, cancellationToken)
				.ConfigureAwait(false);

			if (discEntity.Artist.Id != artistId)
			{
				throw new NotFoundException($"Disc {discId} does not belong to artist {artistId}");
			}

			return discEntity;
		}
	}
}
