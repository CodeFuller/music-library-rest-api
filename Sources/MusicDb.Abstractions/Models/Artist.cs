using System.Collections.Generic;

namespace MusicDb.Abstractions.Models
{
	public class Artist
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public ICollection<Disc> Discs { get; set; }
	}
}
