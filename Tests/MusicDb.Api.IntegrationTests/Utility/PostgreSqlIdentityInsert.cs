using Microsoft.EntityFrameworkCore;
using static System.FormattableString;

namespace MusicDb.Api.IntegrationTests.Utility
{
	public class PostgreSqlIdentityInsert : IIdentityInsert
	{
		public void InitializeIdentityInsert(DbContext context, string tableName)
		{
		}

		public void FinalizeIdentityInsert(DbContext context, string tableName, int nextId)
		{
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
			context.Database.ExecuteSqlCommand(Invariant($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH {nextId}"));
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
		}
	}
}
