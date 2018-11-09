using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicDb.Api;
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
			services
				.AddMvc()
				.AddApplicationPart(typeof(AnchorType).Assembly)
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSwagger();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
