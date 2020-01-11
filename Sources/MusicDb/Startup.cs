using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicDb.Abstractions.Interfaces;
using MusicDb.Api;
using MusicDb.Dal.EfCore;
using MusicDb.Dal.EfCore.Internal;
using MusicDb.Dal.EfCore.Repositories;
using NJsonSchema;
using NSwag.AspNetCore;

namespace MusicDb
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// This call should be made before services.AddMvc().
			services.AddRouting(options => options.LowercaseUrls = true);

			var apiAssembly = typeof(ApiAnchorType).Assembly;

			services
				.AddMvc()
				.AddApplicationPart(apiAssembly)
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSwagger();

			services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();
			services.AddTransient<IArtistsRepository, ArtistsRepository>();
			services.AddTransient<IDiscsRepository, DiscsRepository>();
			services.AddTransient<ISongsRepository, SongsRepository>();
			services.AddTransient<IEntityLocator, EntityLocator>();

			var connectionString = Configuration.GetConnectionString("musicDB");
			if (String.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException("Database connection string is not set");
			}

			services.AddDbContext<MusicDbContext>(options => options.UseNpgsql(connectionString));

			services.AddAutoMapper(apiAssembly);
		}

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
