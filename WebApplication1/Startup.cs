using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication1.Controllers;

namespace WebApplication1
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
            services.AddControllers();
        }

        public void ConfigureContainer(ServiceRegistry services)
        {
            services.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.AssemblyContainingType<ISetter>();
                scanner.WithDefaultConventions();
                scanner.SingleImplementationsOfInterface();
                scanner.Assembly(typeof(Program).Assembly);
                scanner.LookForRegistries();
            });

            var container = new Container(_ =>
            {
                _.For<ISetter>().Use<Setter>();
            });

            services.IncludeRegistry<DerpRegistry>();

            services.For<ISetter>().Use<Setter>();
            services.Policies.SetAllProperties(y => y.OfType<ISetter>());
        }

        public class DerpRegistry : ServiceRegistry
        {
            public DerpRegistry()
            {
                For<ISetter>().Use<Setter>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var container = (IContainer)app.ApplicationServices;

            var plan = container.Model.For<WeatherForecastController>().Default.DescribeBuildPlan();
            
            var whatDidIScan = container.WhatDidIScan();

            Console.WriteLine(plan);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
