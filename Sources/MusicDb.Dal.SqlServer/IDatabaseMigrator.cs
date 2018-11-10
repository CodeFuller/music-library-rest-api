using Microsoft.EntityFrameworkCore;

namespace MusicDb.Dal.SqlServer
{
	public interface IDatabaseMigrator
	{
		void Migrate(DbContext context);
	}
}
