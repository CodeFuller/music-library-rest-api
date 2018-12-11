using Microsoft.EntityFrameworkCore;

namespace MusicDb.Api.IntegrationTests.Utility
{
	public interface IIdentityInsert
	{
		void InitializeIdentityInsert(DbContext context, string tableName);

		void FinalizeIdentityInsert(DbContext context, string tableName, int nextId);
	}
}
