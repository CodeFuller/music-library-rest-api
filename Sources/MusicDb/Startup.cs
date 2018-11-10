using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

			// This call should be made before services.AddMvc().
			services.AddRouting(options => options.LowercaseUrls = true);

			services
				.AddMvc()
				.AddApplicationPart(typeof(AnchorType).Assembly)
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSwagger();

			services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();
			services.AddTransient<IArtistsRepository, ArtistsRepository>();

			var connectionString = Configuration.GetConnectionString("musicDB");
			if (String.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException("Database connection string is not set");
			}

			services.AddDbContext<MusicDbContext>(options => options.UseSqlServer(connectionString));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
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
	}
}
