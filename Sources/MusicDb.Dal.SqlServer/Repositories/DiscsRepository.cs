using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.SqlServer.Repositories
{
	public class DiscsRepository : IDiscsRepository
	{
		private readonly MusicDbContext context;

		private readonly IEntityLocator entityLocator;

		public DiscsRepository(MusicDbContext context, IEntityLocator entityLocator)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			this.entityLocator = entityLocator ?? throw new ArgumentNullException(nameof(entityLocator));
		}

		public async Task<int> CreateDisc(int artistId, Disc disc, CancellationToken cancellationToken)
		{
			var artist = await entityLocator.FindArtist(artistId, true, cancellationToken).ConfigureAwait(false);
			artist.Discs.Add(disc);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return disc.Id;
		}

		public async Task<ICollection<Disc>> GetAllArtistDiscs(int artistId, CancellationToken cancellationToken)
		{
			var artist = await entityLocator.FindArtist(artistId, true, cancellationToken).ConfigureAwait(false);
			return artist.Discs;
		}

		public Task<Disc> GetDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			return entityLocator.FindArtistDisc(artistId, discId, false, cancellationToken);
		}

		public async Task UpdateDisc(int artistId, Disc disc, CancellationToken cancellationToken)
		{
			var discEntity = await entityLocator.FindArtistDisc(artistId, disc.Id, false, cancellationToken).ConfigureAwait(false);
			context.Entry(discEntity).CurrentValues.SetValues(disc);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await entityLocator.FindArtistDisc(artistId, discId, false, cancellationToken).ConfigureAwait(false);
			context.Discs.Remove(discEntity);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
