using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicDb.Dal.EfCore
{
	public class MusicDbContextFactory : IDesignTimeDbContextFactory<MusicDbContext>
	{
		public MusicDbContext CreateDbContext(string[] args)
		{
			var connectionString = LoadConnectionString();

			var optionsBuilder = new DbContextOptionsBuilder<MusicDbContext>();
			optionsBuilder.UseNpgsql(connectionString);

			return new MusicDbContext(optionsBuilder.Options);
		}

		private static string LoadConnectionString()
		{
			// Currently there is not proper way to configure connection string via tool arguments.
			// Track: https://github.com/aspnet/EntityFrameworkCore/issues/8332
			var connectionStringFileName = Path.Combine(Directory.GetCurrentDirectory(), "MigrationsConnectionString.txt");

			if (!File.Exists(connectionStringFileName))
			{
				throw new InvalidOperationException($"File '{connectionStringFileName}' is missing. Create it and put database connection string (one line)");
			}

			var lines = File.ReadAllLines(connectionStringFileName);
			if (lines.Length != 1)
			{
				throw new InvalidOperationException($"File '{connectionStringFileName}' should contain exactly one line with connection string");
			}

			var connectionString = lines.Single();
			Console.WriteLine($"Loaded connection string from '{connectionStringFileName}': '{connectionString}'");

			return connectionString;
		}
	}
}
