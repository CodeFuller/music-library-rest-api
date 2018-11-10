using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Abstractions.Settings;
using MusicDb.Api;
using MusicDb.Dal.SqlServer;
using MusicDb.Dal.SqlServer.Repositories;
using NJsonSchema;
using NSwag.AspNetCore;

namespace MusicDb
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<DatabaseConnectionSettings>(Configuration.GetSection("database"));

			services
				.AddMvc()
				.AddApplicationPart(typeof(AnchorType).Assembly)
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSwagger();

			services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();
			services.AddTransient<IArtistsRepository, ArtistsRepository>();

			// MusicDbContext should be registered as scoped dependency,
			// so that entities loaded within different repositories are the same.
			services.AddScoped<MusicDbContext>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			ApplyDatabseMigrations(app);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSwaggerUi3WithApiExplorer(settings =>
			{
				settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
			});

			app.UseMvc();
		}

		private static void ApplyDatabseMigrations(IApplicationBuilder app)
		{
			var serviceProvider = app.ApplicationServices;

			// Scoped dependency of IndexingManagerContext could not be resolved from root provider.
			// That's why we create it directly
			var dbContext = new MusicDbContext(serviceProvider.GetRequiredService<IOptions<DatabaseConnectionSettings>>());

			var dbMigrator = serviceProvider.GetRequiredService<IDatabaseMigrator>();
			dbMigrator.Migrate(dbContext);
		}
	}
}
