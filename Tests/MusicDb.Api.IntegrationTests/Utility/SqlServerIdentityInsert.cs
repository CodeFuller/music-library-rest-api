using Microsoft.EntityFrameworkCore;

namespace MusicDb.Api.IntegrationTests.Utility
{
	public class SqlServerIdentityInsert : IIdentityInsert
	{
		public void InitializeIdentityInsert(DbContext context, string tableName)
		{
			// https://docs.microsoft.com/en-us/ef/core/saving/explicit-values-generated-properties#explicit-values-into-sql-server-identity-columns
			context.Database.ExecuteSqlCommand($"SET IDENTITY_INSERT dbo.{tableName} ON");
			context.Database.ExecuteSqlCommand($"DBCC CHECKIDENT ('{tableName}', RESEED, 1)");
		}

		public void FinalizeIdentityInsert(DbContext context, string tableName, int nextId)
		{
			context.Database.ExecuteSqlCommand($"SET IDENTITY_INSERT dbo.{tableName} OFF");
		}
	}
}
