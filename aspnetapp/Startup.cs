using System;
using System.Diagnostics;
using System.Threading.Tasks;
using aspnetapp.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace aspnetapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) => services.AddSingleton<RootMiddleware>();

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var root = app.ApplicationServices.GetService<RootMiddleware>();
            app.Run(c =>
            {
                root.InvokeAsync(c);
                return Task.CompletedTask;
            });
        }
    }
}