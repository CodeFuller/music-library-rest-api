using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer;

namespace MusicDb.Api.IntegrationTests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
	{
		private DbConnection connection;

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				services.AddDbContext<MusicDbContext>(SetupDbContextOptions);
			});
		}

		public void SeedData()
		{
			var optionsBuilder = new DbContextOptionsBuilder();
			SetupDbContextOptions(optionsBuilder);

			using (var initContext = new MusicDbContext(optionsBuilder.Options))
			{
				initContext.Database.EnsureCreated();
				SeedData(initContext);
			}
		}

		private void SetupDbContextOptions(DbContextOptionsBuilder optionsBuilder)
		{
			if (connection == null)
			{
				connection = new SqliteConnection("DataSource=:memory:");
				connection.Open();
			}

			optionsBuilder.UseSqlite(connection);
		}

		private static void SeedData(MusicDbContext context)
		{
			var artist1 = new Artist
			{
				Id = 1,
				Name = "Korn",
			};

			var artist2 = new Artist
			{
				Id = 2,
				Name = "Guano Apes",
			};

			context.Artists.RemoveRange(context.Artists);
			context.Artists.AddRange(artist1, artist2);

			context.SaveChanges();
		}
	}
}
