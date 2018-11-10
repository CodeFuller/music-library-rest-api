using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MusicDb.Abstractions.Models;
using MusicDb.Abstractions.Settings;

namespace MusicDb.Dal.SqlServer
{
	public class MusicDbContext : DbContext
	{
		private readonly string connectionString;

		public DbSet<Artist> Artists { get; set; }

		public DbSet<Disc> Discs { get; set; }

		public MusicDbContext(IOptions<DatabaseConnectionSettings> options)
		{
			connectionString = options?.Value?.ConnectionString ?? throw new ArgumentNullException(nameof(options));

			if (String.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException("Database connection string is not set");
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(connectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Artist>().Property(e => e.Name).IsRequired();
			modelBuilder.Entity<Artist>().HasIndex(e => e.Name).IsUnique();

			modelBuilder.Entity<Disc>().Property(e => e.Title).IsRequired();
			modelBuilder.Entity<Disc>()
				.HasOne(d => d.Artist)
				.WithMany(a => a.Discs)
				.IsRequired();
		}
	}
}
