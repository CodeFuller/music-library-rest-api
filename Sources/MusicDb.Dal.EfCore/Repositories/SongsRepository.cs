using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.EfCore.Repositories
{
	public class SongsRepository : ISongsRepository
	{
		private readonly MusicDbContext context;

		private readonly IEntityLocator entityLocator;

		public SongsRepository(MusicDbContext context, IEntityLocator entityLocator)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			this.entityLocator = entityLocator ?? throw new ArgumentNullException(nameof(entityLocator));
		}

		public async Task<int> CreateSong(int artistId, int discId, Song song, CancellationToken cancellationToken)
		{
			var disc = await entityLocator.FindArtistDisc(artistId, discId, true, cancellationToken).ConfigureAwait(false);
			disc.Songs.Add(song);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return song.Id;
		}

		public async Task<ICollection<Song>> GetAllDiscSongs(int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await entityLocator.FindArtistDisc(artistId, discId, true, cancellationToken).ConfigureAwait(false);
			return discEntity.Songs;
		}

		public Task<Song> GetSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			return entityLocator.FindDiscSong(artistId, discId, songId, cancellationToken);
		}

		public async Task UpdateSong(int artistId, int discId, Song song, CancellationToken cancellationToken)
		{
			var songEntity = await entityLocator.FindDiscSong(artistId, discId, song.Id, cancellationToken).ConfigureAwait(false);
			context.Entry(songEntity).CurrentValues.SetValues(song);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			var songEntity = await entityLocator.FindDiscSong(artistId, discId, songId, cancellationToken).ConfigureAwait(false);
			context.Songs.Remove(songEntity);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
