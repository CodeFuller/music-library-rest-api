using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicDb.Abstractions.Models;

namespace MusicDb.Abstractions.Interfaces
{
	public interface IArtistsRepository
	{
		Task<int> CreateArtist(Artist artist, CancellationToken cancellationToken);

		Task<ICollection<Artist>> GetAllArtists(CancellationToken cancellationToken);

		Task<Artist> GetArtist(int artistId, CancellationToken cancellationToken);

		Task UpdateArtist(Artist artist, CancellationToken cancellationToken);

		Task DeleteArtist(int artistId, CancellationToken cancellationToken);
	}
}
