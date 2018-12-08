using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicDb.Abstractions.Models;
using MusicDb.Dal.SqlServer;

namespace MusicDb.Api.IntegrationTests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
	{
		private DatabaseSettings dbSettings;

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
				var configBuilder = new ConfigurationBuilder();

				// By default SQLite configuration is used
				// CI Build deletes TestRunSettings.SQLite.json before running the test and TestRunSettings.SqlServer.json is used.
				configBuilder.AddJsonFile("TestRunSettings.SqlServer.json", optional: true)
					.AddJsonFile("TestRunSettings.SQLite.json", optional: true);
				var configuration = configBuilder.Build();

				dbSettings = new DatabaseSettings();
				configuration.Bind("database", dbSettings);
				if (String.IsNullOrEmpty(dbSettings.ConnectionString))
				{
					throw new InvalidOperationException("Database connection string is not set");
				}

				connection = CreateDbConnection(dbSettings.ConnectionString);
				connection.Open();
			}

			var(_, dbContextOptionsConfig) = GetDbInitializers();
			dbContextOptionsConfig(connection, optionsBuilder);
		}

		private DbConnection CreateDbConnection(string connectionString)
		{
			var(connectionFactory, _) = GetDbInitializers();
			return connectionFactory(connectionString);
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private (Func<string, DbConnection>, Action<DbConnection, DbContextOptionsBuilder>) GetDbInitializers()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			switch (dbSettings.DbProviderType)
			{
				case DbProviderType.Sqlite:
					return (cs => new SqliteConnection(cs), (conn, ob) => ob.UseSqlite(conn));

				case DbProviderType.SqlServer:
					return (cs => new SqlConnection(cs), (conn, ob) => ob.UseSqlServer(conn));

				default:
					throw new InvalidOperationException($"DB Provider {dbSettings.DbProviderType} is not supported");
			}
		}

		private void SeedData(MusicDbContext context)
		{
			var setIdentityInsert = dbSettings.DbProviderType == DbProviderType.SqlServer;

			// Deleting any existing data.
			// This should be done before resetting currenty identity value.
			context.Artists.RemoveRange(context.Artists);
			context.SaveChanges();

			SeedArtistsData(context, setIdentityInsert);
			SeedDiscsData(context, setIdentityInsert);
			SeedSongsData(context, setIdentityInsert);
		}

		private void SeedArtistsData(MusicDbContext context, bool setIdentityInsert)
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

			if (setIdentityInsert)
			{
				// https://docs.microsoft.com/en-us/ef/core/saving/explicit-values-generated-properties#explicit-values-into-sql-server-identity-columns
				context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Artists ON");
				context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Artists', RESEED, 1)");
			}

			context.Artists.AddRange(artist1, artist2);
			context.SaveChanges();

			if (setIdentityInsert)
			{
				context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Artists OFF");
			}
		}

		private void SeedDiscsData(MusicDbContext context, bool setIdentityInsert)
		{
			var disc11 = new Disc
			{
				Id = 11,
				Title = "Follow The Leader",
				Year = 1998,
			};

			var disc21 = new Disc
			{
				Id = 21,
				Title = "Proud Like A God",
				Year = 1997,
			};

			var disc22 = new Disc
			{
				Id = 22,
				Title = "Don't Give Me Names",
				Year = null, // This disc is intentionally left with blank year
			};

			if (setIdentityInsert)
			{
				context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Discs ON");
				context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Discs', RESEED, 1)");
			}

			var artistsQueryAble = context.Artists
				.Include(a => a.Discs);

			artistsQueryAble.Single(a => a.Id == 1)
				.Discs.Add(disc11);

			artistsQueryAble.Single(a => a.Id == 2)
				.Discs.AddRange(new[] { disc21, disc22 });

			context.SaveChanges();

			if (setIdentityInsert)
			{
				context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Discs OFF");
			}
		}

		private void SeedSongsData(MusicDbContext context, bool setIdentityInsert)
		{
			var song111 = new Song
			{
				Id = 111,
				Title = "It's On!",
				TrackNumber = 1,
				Duration = new TimeSpan(0, 4, 28),
			};

			var song211 = new Song
			{
				Id = 21,
				Title = "Open Your Eyes",
				TrackNumber = 1,
				Duration = new TimeSpan(0, 3, 9),
			};

			var song221 = new Song
			{
				Id = 221,
				Title = "Innocent Greed",
				TrackNumber = 1,
				Duration = new TimeSpan(0, 3, 51),
			};

			var song222 = new Song
			{
				Id = 222,
				Title = "No Speech",
				TrackNumber = null, // This data was intentionally left blank.
				Duration = null,    // This data was intentionally left blank.
			};

			if (setIdentityInsert)
			{
				context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Songs ON");
				context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Songs', RESEED, 1)");
			}

			var discs = context.Discs
				.Include(a => a.Songs);

			discs.Single(a => a.Id == 11)
				.Songs.Add(song111);

			discs.Single(a => a.Id == 21)
				.Songs.Add(song211);

			discs.Single(a => a.Id == 22)
				.Songs.AddRange(new[] { song221, song222 });

			context.SaveChanges();

			if (setIdentityInsert)
			{
				context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Songs OFF");
			}
		}
	}
}
