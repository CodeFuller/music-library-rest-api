using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.SqlServer.Repositories
{
	public class DiscsRepository : BasicRepository, IDiscsRepository
	{
		public DiscsRepository(MusicDbContext context)
			: base(context)
		{
		}

		public async Task CreateDisc(int artistId, Disc disc, CancellationToken cancellationToken)
		{
			var artist = await FindArtist(artistId, includeDiscs: true, cancellationToken: cancellationToken).ConfigureAwait(false);
			artist.Discs.Add(disc);

			await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task<ICollection<Disc>> GetAllArtistDiscs(int artistId, CancellationToken cancellationToken)
		{
			var artist = await FindArtist(artistId, includeDiscs: true, cancellationToken: cancellationToken).ConfigureAwait(false);

			return artist.Discs;
		}

		public async Task<Disc> GetDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			return await FindArtistDisc(artistId, discId, cancellationToken).ConfigureAwait(false);
		}

		public async Task UpdateDisc(int artistId, Disc disc, CancellationToken cancellationToken)
		{
			var discEntity = await FindArtistDisc(artistId, disc.Id, cancellationToken).ConfigureAwait(false);
			Context.Entry(discEntity).CurrentValues.SetValues(disc);

			await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await FindArtistDisc(artistId, discId, cancellationToken).ConfigureAwait(false);
			Context.Discs.Remove(discEntity);

			await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private async Task<Disc> FindDisc(int discId, CancellationToken cancellationToken)
		{
			var discEntity = await Context.Discs
				.Include(d => d.Artist)
				.Where(a => a.Id == discId)
				.FirstOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (discEntity == null)
			{
				throw new NotFoundException($"Disc with id of {discId} was not found");
			}

			return discEntity;
		}

		private async Task<Disc> FindArtistDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await FindDisc(discId, cancellationToken).ConfigureAwait(false);
			if (discEntity.Artist.Id != artistId)
			{
				throw new NotFoundException($"Disc {discId} does not belong to artist {artistId}");
			}

			return discEntity;
		}
	}
}
