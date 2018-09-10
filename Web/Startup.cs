using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Template.Bll;
using Template.Dal;

namespace Template.Web
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
            var connectionString = Configuration.GetConnectionString("EmployeeInfo");
            services.AddDbContext<EmployeeContext>(builder =>
            {                
                builder.UseSqlServer(connectionString);
            });
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var apiPath = new PathString("/api");
            app.UseStaticFiles("/content");
            app.UseWhen(context =>
                context.Request.Path.StartsWithSegments(apiPath, StringComparison.InvariantCultureIgnoreCase),
                a => ApiConfigure(a, env));
            app.UseWhen(context =>
                !context.Request.Path.StartsWithSegments(apiPath, StringComparison.InvariantCultureIgnoreCase),
                a => UiConfigure(a, env));
            app.UseAuthentication();
            app.UseCustomAuthorization(options => { });
            app.UseMvc();
        }

        private void ApiConfigure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseApiExceptionHandler();
        }

        private void UiConfigure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler();
        }
    }
}
