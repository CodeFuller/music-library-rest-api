using System;

namespace MusicDb.Abstractions.Models
{
	public class Song
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public short? TrackNumber { get; set; }

		public TimeSpan? Duration { get; set; }

		public Disc Disc { get; set; }
	}
}
