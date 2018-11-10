using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MusicDb.Dal.SqlServer;

namespace MusicDb
{
	public static class WebHostExtensions
	{
		public static IWebHost ApplyMigrations(this IWebHost webHost)
		{
			var appServiceProvider = webHost.Services;
			using (var serviceScope = appServiceProvider.CreateScope())
			{
				var scopeServiceProvider = serviceScope.ServiceProvider;
				using (var dbContext = scopeServiceProvider.GetRequiredService<MusicDbContext>())
				{
					var dbMigrator = scopeServiceProvider.GetRequiredService<IDatabaseMigrator>();
					dbMigrator.Migrate(dbContext);
				}
			}

			return webHost;
		}
	}
}
