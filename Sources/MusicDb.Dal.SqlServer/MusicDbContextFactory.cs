using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using MusicDb.Abstractions.Settings;

namespace MusicDb.Dal.SqlServer
{
	public class MusicDbContextFactory : IDesignTimeDbContextFactory<MusicDbContext>
	{
		public MusicDbContext CreateDbContext(string[] args)
		{
			var settings = new DatabaseConnectionSettings
			{
				// Currently there is not proper way to configure connection string via tool arguments.
				// Track: https://github.com/aspnet/EntityFrameworkCore/issues/8332
				ConnectionString = @"Data Source=localhost;Initial Catalog=MusicDB;Integrated Security=True",
			};

			return new MusicDbContext(Options.Create(settings));
		}
	}
}
