using System;
using System.Data.Common;
using System.Data.SqlClient;
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
				configBuilder.AddJsonFile("TestRunSettings.json", optional: false);
				var configuration = configBuilder.Build();

				dbSettings = new DatabaseSettings();
				configuration.Bind("database", dbSettings);

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
			// Deleting any existing data.
			// This should be done before resetting currenty identity value.
			context.Artists.RemoveRange(context.Artists);
			context.SaveChanges();

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

			var setIdentityInsert = dbSettings.DbProviderType == DbProviderType.SqlServer;
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
	}
}
