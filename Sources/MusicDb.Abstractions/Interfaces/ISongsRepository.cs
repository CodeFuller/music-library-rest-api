using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicDb.Abstractions.Models;

namespace MusicDb.Abstractions.Interfaces
{
	public interface ISongsRepository
	{
		Task<int> CreateSong(int artistId, int discId, Song song, CancellationToken cancellationToken);

		Task<ICollection<Song>> GetAllDiscSongs(int artistId, int discId, CancellationToken cancellationToken);

		Task<Song> GetSong(int artistId, int discId, int songId, CancellationToken cancellationToken);

		Task UpdateSong(int artistId, int discId, Song song, CancellationToken cancellationToken);

		Task DeleteSong(int artistId, int discId, int songId, CancellationToken cancellationToken);
	}
}
