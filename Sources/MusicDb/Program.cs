using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MusicDb
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			await CreateWebHostBuilder(args)
				.Build()
				.ApplyMigrations()
				.RunAsync()
				.ConfigureAwait(false);
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
