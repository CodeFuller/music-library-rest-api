using Microsoft.EntityFrameworkCore;

namespace MusicDb.Api.IntegrationTests.Utility
{
	public class SqliteIdentityInsert : IIdentityInsert
	{
		public void InitializeIdentityInsert(DbContext context, string tableName)
		{
		}

		public void FinalizeIdentityInsert(DbContext context, string tableName, int nextId)
		{
		}
	}
}
