using Microsoft.EntityFrameworkCore;

namespace MusicDb.Dal.SqlServer.Tests.Utility
{
	internal static class DbContextOptionsExtensions
	{
		public static MusicDbContext ToContext(this DbContextOptions<MusicDbContext> options)
		{
			return new MusicDbContext(options);
		}
	}
}
