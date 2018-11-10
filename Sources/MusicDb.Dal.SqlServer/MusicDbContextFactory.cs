using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicDb.Dal.SqlServer
{
	public class MusicDbContextFactory : IDesignTimeDbContextFactory<MusicDbContext>
	{
		public MusicDbContext CreateDbContext(string[] args)
		{
			// Currently there is not proper way to configure connection string via tool arguments.
			// Track: https://github.com/aspnet/EntityFrameworkCore/issues/8332
			var optionsBuilder = new DbContextOptionsBuilder<MusicDbContext>();
			optionsBuilder.UseSqlServer(@"Data Source=localhost;Initial Catalog=MusicDB;Integrated Security=True");

			return new MusicDbContext(optionsBuilder.Options);
		}
	}
}
