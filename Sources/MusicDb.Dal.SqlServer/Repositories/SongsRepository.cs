using System;
using System.Collections.Generic;
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
	public class SongsRepository : ISongsRepository
	{
		private readonly MusicDbContext context;

		public SongsRepository(MusicDbContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<int> CreateSong(int artistId, int discId, Song song, CancellationToken cancellationToken)
		{
			var disc = await FindDisc(artistId, discId, cancellationToken).ConfigureAwait(false);
			disc.Songs.Add(song);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return song.Id;
		}

		public async Task<ICollection<Song>> GetAllDiscSongs(int artistId, int discId, CancellationToken cancellationToken)
		{
			var discEntity = await FindDisc(artistId, discId, cancellationToken)
				.ConfigureAwait(false);

			return discEntity.Songs;
		}

		public async Task<Song> GetSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			return await FindDiscSong(artistId, discId, songId, cancellationToken).ConfigureAwait(false);
		}

		public async Task UpdateSong(int artistId, int discId, Song song, CancellationToken cancellationToken)
		{
			var songEntity = await FindDiscSong(artistId, discId, song.Id, cancellationToken).ConfigureAwait(false);
			context.Entry(songEntity).CurrentValues.SetValues(song);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			var songEntity = await FindDiscSong(artistId, discId, songId, cancellationToken).ConfigureAwait(false);
			context.Songs.Remove(songEntity);

			await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		private Task<Disc> FindDisc(int artistId, int discId, CancellationToken cancellationToken)
		{
			return context.Discs
				.Include(d => d.Artist)
				.Include(d => d.Songs)
				.FindArtistDisc(artistId, discId, cancellationToken);
		}

		private async Task<Song> FindSong(int songId, CancellationToken cancellationToken)
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

			return songEntity;
		}

		private async Task<Song> FindDiscSong(int artistId, int discId, int songId, CancellationToken cancellationToken)
		{
			var songEntity = await FindSong(songId, cancellationToken).ConfigureAwait(false);

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
