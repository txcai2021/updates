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

namespace SIMTech.APS.Integration.API
{
    using SIMTech.APS.Integration.RabbitMQ;
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

            var rabbit_enable = Environment.GetEnvironmentVariable("RABBITMQ_ENABLE");

            if (rabbit_enable != null && rabbit_enable == "100")
            {
                services.AddHostedService<CustomerConsumer>();
                services.AddHostedService<SalesOrderConsumer>();
                services.AddHostedService<InventoryConsumer>();
                services.AddHostedService<WorkOrderStatusConsumer>();
                services.AddHostedService<ProcessStatusConsumer>();
                services.AddHostedService<MachineBlockoutConsumer>();
            }
            else
            {
                Console.WriteLine("RabbitMQ is disabled");
            }

           
            
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIMTech.APS.Integration.API", Version = "v1" });
            });
      
            services.AddDbContext<DBContext.IntegrationContext>(o => o.UseSqlServer(Environment.GetEnvironmentVariable("RPS_DB_INTEGRATION")));
            //services.AddDbContext<DBContext.IntegrationContext>(o => o.UseSqlServer(Configuration.GetConnectionString("IntegrationDB")).UseLazyLoadingProxies());

            services.AddScoped<Repository.IPPOrderRepository, Repository.PPOrderRepository>();

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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIMTech.APS.Integration.API v1"));
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

