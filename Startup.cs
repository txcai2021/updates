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

namespace SIMTech.APS.Operation.API
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

            //services.AddControllers(options =>
            //{
            //    options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
            //    options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(new JsonSerializerOptions(JsonSerializerDefaults.Web)
            //    {
            //        ReferenceHandler = ReferenceHandler.Preserve,
            //    }));
            //});

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIMTech.APS.Operation.API", Version = "v1" });
            });

            var dbConnetionString = Environment.GetEnvironmentVariable("RPS_DB_OPERATION");

            if (dbConnetionString != null && dbConnetionString.Length > 150)
            {
                var wrapper = new SIMTech.APS.Utilities.DataProtection("#RPS_DB_OPERATION#");
                dbConnetionString = wrapper.DecryptData(dbConnetionString);
            }
            services.AddDbContext<DBContext.OperationContext>(o => o.UseSqlServer(dbConnetionString));
            //services.AddDbContext<DBContext.OperationContext>(o => o.UseSqlServer(Environment.GetEnvironmentVariable("RPS_DB_OPERATION")));

            services.AddScoped<Repository.IOperationRepository, Repository.OperationRepository>();
            services.AddScoped<Repository.IOperationResourceRepository, Repository.OperationResourceRepository>();
            services.AddScoped<Repository.IOperationParameterRepository, Repository.OperationParameterRepository>();
            services.AddScoped<Repository.IOperationRateRepository, Repository.OperationRateRepository>();
            services.AddScoped<Repository.IParameterRepository, Repository.ParameterRepository>();

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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIMTech.APS.Operation.API v1"));
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

