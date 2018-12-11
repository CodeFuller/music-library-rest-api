using System.Threading;
using System.Threading.Tasks;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.EfCore
{
	public interface IEntityLocator
	{
		Task<Artist> FindArtist(int artistId, bool includeDiscs, CancellationToken cancellationToken);

		Task<Disc> FindArtistDisc(int artistId, int discId, bool includeSongs, CancellationToken cancellationToken);

		Task<Song> FindDiscSong(int artistId, int discId, int songId, CancellationToken cancellationToken);
	}
}
