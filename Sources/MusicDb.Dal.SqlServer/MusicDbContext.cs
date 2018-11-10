using Microsoft.EntityFrameworkCore;
using MusicDb.Abstractions.Models;

namespace MusicDb.Dal.SqlServer
{
	public class MusicDbContext : DbContext
	{
		public DbSet<Artist> Artists { get; set; }

		public DbSet<Disc> Discs { get; set; }

		public MusicDbContext(DbContextOptions<MusicDbContext> options)
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
		}
	}
}
