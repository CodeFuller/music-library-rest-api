using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicDb.Abstractions.Models;

namespace MusicDb.Abstractions.Interfaces
{
	public interface IDiscsRepository
	{
		Task<int> CreateDisc(int artistId, Disc disc, CancellationToken cancellationToken);

		Task<ICollection<Disc>> GetAllArtistDiscs(int artistId, CancellationToken cancellationToken);

		Task<Disc> GetDisc(int artistId, int discId, CancellationToken cancellationToken);

		Task UpdateDisc(int artistId, Disc disc, CancellationToken cancellationToken);

		Task DeleteDisc(int artistId, int discId, CancellationToken cancellationToken);
	}
}
