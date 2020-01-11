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
using MusicDb.Api.IntegrationTests.Utility;
using MusicDb.Dal.EfCore;
using Npgsql;

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
				services.AddDbContext<MusicDbContext>(optionsBuilder => SetupDbContextOptions(optionsBuilder));
			});
		}

		public void SeedData()
		{
			var optionsBuilder = new DbContextOptionsBuilder();
			var identityInsert = SetupDbContextOptions(optionsBuilder);

			using (var initContext = new MusicDbContext(optionsBuilder.Options))
			{
				initContext.Database.EnsureCreated();
				SeedData(initContext, identityInsert);
			}
		}

		private IIdentityInsert SetupDbContextOptions(DbContextOptionsBuilder optionsBuilder)
		{
			if (connection == null)
			{
				var configBuilder = new ConfigurationBuilder();

				// By default SQLite configuration is used
				// CI Build deletes TestRunSettings.SQLite.json before running the test and TestRunSettings.PostgreSql.json is used.
				configBuilder.AddJsonFile("TestRunSettings.PostgreSql.json", optional: true)
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

			var(_, dbContextOptionsConfig, identityInsert) = GetDbInitializers();
			dbContextOptionsConfig(connection, optionsBuilder);

			return identityInsert;
		}

		private DbConnection CreateDbConnection(string connectionString)
		{
			var(connectionFactory, _, _) = GetDbInitializers();
			return connectionFactory(connectionString);
		}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
		private (Func<string, DbConnection>, Action<DbConnection, DbContextOptionsBuilder>, IIdentityInsert) GetDbInitializers()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
		{
			switch (dbSettings.DbProviderType)
			{
				case DbProviderType.Sqlite:
					return (cs => new SqliteConnection(cs), (conn, ob) => ob.UseSqlite(conn), new SqliteIdentityInsert());

				case DbProviderType.SqlServer:
					return (cs => new SqlConnection(cs), (conn, ob) => ob.UseSqlServer(conn), new SqlServerIdentityInsert());

				case DbProviderType.PostgreSql:
					return (cs => new NpgsqlConnection(cs), (conn, ob) => ob.UseNpgsql(conn), new PostgreSqlIdentityInsert());

				default:
					throw new InvalidOperationException($"DB Provider {dbSettings.DbProviderType} is not supported");
			}
		}

		private void SeedData(MusicDbContext context, IIdentityInsert identityInsert)
		{
			// Deleting any existing data.
			// This should be done before resetting current identity value.
			context.Artists.RemoveRange(context.Artists);
			context.SaveChanges();

			SeedArtistsData(context, identityInsert);
			SeedDiscsData(context, identityInsert);
			SeedSongsData(context, identityInsert);
		}

		private void SeedArtistsData(MusicDbContext context, IIdentityInsert identityInsert)
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

			identityInsert.InitializeIdentityInsert(context, "Artists");

			context.Artists.AddRange(artist1, artist2);
			context.SaveChanges();

			identityInsert.FinalizeIdentityInsert(context, "Artists", 3);
		}

		private void SeedDiscsData(MusicDbContext context, IIdentityInsert identityInsert)
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

			identityInsert.InitializeIdentityInsert(context, "Discs");

			var artistsQueryAble = context.Artists
				.Include(a => a.Discs);

			artistsQueryAble.Single(a => a.Id == 1)
				.Discs.Add(disc11);

			artistsQueryAble.Single(a => a.Id == 2)
				.Discs.AddRange(new[] { disc21, disc22 });

			context.SaveChanges();

			identityInsert.FinalizeIdentityInsert(context, "Discs", 23);
		}

		private void SeedSongsData(MusicDbContext context, IIdentityInsert identityInsert)
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

			identityInsert.InitializeIdentityInsert(context, "Songs");

			var discs = context.Discs
				.Include(a => a.Songs);

			discs.Single(a => a.Id == 11)
				.Songs.Add(song111);

			discs.Single(a => a.Id == 21)
				.Songs.Add(song211);

			discs.Single(a => a.Id == 22)
				.Songs.AddRange(new[] { song221, song222 });

			context.SaveChanges();

			identityInsert.FinalizeIdentityInsert(context, "Songs", 223);
		}
	}
}
