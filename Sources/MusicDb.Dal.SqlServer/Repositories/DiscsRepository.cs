using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer.Internal;

namespace MusicDb.Dal.SqlServer.Repositories
{
	public class DiscsRepository : IDiscsRepository
	{
		private readonly MusicDbContext context;

		public DiscsRepository(MusicDbContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<int> CreateDisc(int artistId, Disc disc, CancellationToken cancellationToken)
		{
			var artist = await FindArtist(artistId, cancellationToken).ConfigureAwait(false);
			artist.Discs.Add(disc);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return disc.Id;
		}

		public async Task<ICollection<Disc>> GetAllArtistDiscs(int artistId, CancellationToken cancellationToken)
		{
			var artist = await FindArtist(artistId, cancellationToken).ConfigureAwait(false);

			return artist.Discs;
		}

		public async Task<Disc> GetDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			return await FindArtistDisc(artistId, discId, cancellationToken).ConfigureAwait(false);
		}

		public async Task UpdateDisc(int artistId, Disc disc, CancellationToken cancellationToken)
		{
			var discEntity = await FindArtistDisc(artistId, disc.Id, cancellationToken).ConfigureAwait(false);
			context.Entry(discEntity).CurrentValues.SetValues(disc);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await FindArtistDisc(artistId, discId, cancellationToken).ConfigureAwait(false);
			context.Discs.Remove(discEntity);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private Task<Artist> FindArtist(int artistId, CancellationToken cancellationToken)
		{
			return context.Artists
				.Include(a => a.Discs)
				.FindArtist(artistId, cancellationToken);
		}

		private Task<Disc> FindArtistDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			return context.Discs
				.Include(d => d.Artist)
				.FindArtistDisc(artistId, discId, cancellationToken);
		}
	}
}
