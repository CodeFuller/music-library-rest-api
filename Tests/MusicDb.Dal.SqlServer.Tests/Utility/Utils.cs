using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MusicDb.Dal.SqlServer.Tests.Utility
{
	internal static class Utils
	{
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		public static (MusicDbContext, DbContextOptions<MusicDbContext>) CreateTestContext()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			var connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();

			var options = new DbContextOptionsBuilder<MusicDbContext>()
				.UseSqlite(connection)
				.Options;

			using (var initContext = new MusicDbContext(options))
			{
				initContext.Database.EnsureCreated();
			}

			return (new MusicDbContext(options), options);
		}
	}
}
