using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.SqlServer.Tests.Utility
{
	internal static class MusicDbContextExtensions
	{
		public static MusicDbContext WithArtists(this MusicDbContext context, params Artist[] artists)
		{
			foreach (var artist in artists)
			{
				context.Artists.Add(artist);
			}

			return context;
		}
	}
}
