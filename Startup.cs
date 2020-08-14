using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;

using Aiello_Restful_API.Models;
using Aiello_Restful_API.Config;
using Aiello_Restful_API.Controllers;
using Aiello_Restful_API.ORM;
//using Aiello_Restful_API.Data;
using Neo4j.Driver;

namespace Aiello_Restful_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //Dependency Ingestions
            services.AddScoped<CityCypher>();
            services.AddScoped<HotelCypher>();
            services.AddScoped<DomainCypher>();
            services.AddScoped<RoomCypher>();
            services.AddScoped<RoomStateCypher>();
            //services.AddScoped<FloorCypher>();
            //services.AddScoped<DeviceStatusCypher>();
            //services.AddScoped<DeviceCypher>();

            //Neo4j DB
            IConfiguration state = Configuration.GetSection("Neo4jTesting");           
            services.AddSingleton(GraphDatabase.Driver(state["URL"], AuthTokens.Basic(state["ID"], state["PW"])));

            //EF core
            //services.AddDbContext<TableContext>(options =>
            //{
            //    options.UseSqlServer(Configuration.GetConnectionString("TableContext"));
            //});

            // Register the Swagger services
            services.AddSwaggerDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints
            });

            //EF core
            //tableContext.Database.EnsureCreated();    
           

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}
