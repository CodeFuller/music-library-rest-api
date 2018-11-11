namespace MusicDb.Abstractions.Models
{
	public class Disc
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public int? Year { get; set; }

		public Artist Artist { get; set; }
	}
}
