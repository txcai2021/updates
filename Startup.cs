using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Cors.Infrastructure;


namespace SIMTech.APS.Resource.API
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIMTech.APS.Resource.API", Version = "v1" });
            });

            var dbConnetionString = Environment.GetEnvironmentVariable("RPS_DB_RESOURCE");

            if (dbConnetionString != null && dbConnetionString.Length > 150)
            {
                var wrapper = new SIMTech.APS.Utilities.DataProtection("#RPS_DB_RESOURCE#");
                dbConnetionString = wrapper.DecryptData(dbConnetionString);
            }
            services.AddDbContext<DBContext.ResourceContext>(o => o.UseSqlServer(dbConnetionString).UseLazyLoadingProxies());

            //services.AddDbContext<DBContext.ResourceContext>(o => o.UseSqlServer(Environment.GetEnvironmentVariable("RPS_DB_RESOURCE")).UseLazyLoadingProxies());
            services.AddScoped<Repository.IResourceRepository, Repository.ResourceRepository>();
            services.AddScoped<Repository.IResourceBlockoutRepository, Repository.ResourceBlockoutRepository>();
            services.AddScoped<Repository.IResourceParameterRepository, Repository.ResourceParameterRepository>();

            services.AddCors(c =>
            {
                //c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
                c.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins(Environment.GetEnvironmentVariable("RPS_UI_URL"))
                                        .AllowAnyHeader()
                                                  .AllowAnyMethod();
                                  });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIMTech.APS.Resource.API v1"));
            }

            //app.UseCors(options => options.AllowAnyOrigin());

            app.UseCors(MyAllowSpecificOrigins);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}