using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

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
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddJsonFile("appsettings.json", optional: false)
						.AddEnvironmentVariables()
						.AddCommandLine(args);
				})
				.UseStartup<Startup>();
	}
}
