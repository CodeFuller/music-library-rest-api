using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Exceptions;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.EfCore.Internal
{
	public class EntityLocator : IEntityLocator
	{
		private readonly MusicDbContext context;

		public EntityLocator(MusicDbContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<Artist> FindArtist(int artistId, bool includeDiscs, CancellationToken cancellationToken)
		{
			var artists = context.Artists.AsQueryable();

			if (includeDiscs)
			{
				artists = artists.Include(a => a.Discs);
			}

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

		public async Task<Disc> FindArtistDisc(int artistId, int discId, bool includeSongs, CancellationToken cancellationToken)
		{
			IQueryable<Disc> discs = context.Discs
				.Include(d => d.Artist);

			if (includeSongs)
			{
				discs = discs.Include(d => d.Songs);
			}

			var discEntity = await discs
				.Where(d => d.Id == discId)
				.SingleOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (discEntity == null)
			{
				throw new NotFoundException($"Disc {discId} was not found");
			}

			if (discEntity.Artist.Id != artistId)
			{
				throw new NotFoundException($"Disc {discId} does not belong to artist {artistId}");
			}

			return discEntity;
		}

		public async Task<Song> FindDiscSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			var songEntity = await context.Songs
				.Include(s => s.Disc)
				.ThenInclude(d => d.Artist)
				.Where(s => s.Id == songId)
				.SingleOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);

			if (songEntity == null)
			{
				throw new NotFoundException($"Song {songId} was not found");
			}

			var disc = songEntity.Disc;
			if (discId != disc.Id)
			{
				throw new NotFoundException($"Song {songId} does not belong to disc {discId}");
			}

			var artist = disc.Artist;
			if (artistId != artist.Id)
			{
				throw new NotFoundException($"Disc {discId} does not belong to artist {artistId}");
			}

			return songEntity;
		}
	}
}
