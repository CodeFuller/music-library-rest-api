﻿using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.EfCore
{
	public class MusicDbContext : DbContext
	{
		public DbSet<Artist> Artists { get; set; }

		public DbSet<Disc> Discs { get; set; }

		public DbSet<Song> Songs { get; set; }

		public MusicDbContext(DbContextOptions options)
			: base(options)
		{
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

			modelBuilder.Entity<Song>().Property(e => e.Title).IsRequired();
			modelBuilder.Entity<Song>()
				.HasOne(s => s.Disc)
				.WithMany(d => d.Songs)
				.IsRequired();
		}
	}
}
