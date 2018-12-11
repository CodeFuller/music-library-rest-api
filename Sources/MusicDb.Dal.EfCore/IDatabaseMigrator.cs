using Microsoft.EntityFrameworkCore;

namespace MusicDb.Dal.EfCore
{
	public interface IDatabaseMigrator
	{
		void Migrate(DbContext context);
	}
}
