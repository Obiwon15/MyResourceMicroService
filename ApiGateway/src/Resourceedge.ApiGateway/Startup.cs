using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Resourceedge.ApiGateway.Services.Employee;
using Resourceedge.Common;
using Resourceedge.Common.Consul;
using Resourceedge.Common.Dispatchers;
using Resourceedge.Common.Jaeger;
using Resourceedge.Common.Mvc;
using Resourceedge.Common.RabbitMq;
using Resourceedge.Common.RestEase;
using Resourceedge.Common.Swagger;

namespace Resourceedge.ApiGateway
{
    public class Startup
    {
        private static readonly string[] Headers = new[] { "X-Operation", "X-Resource", "X-Total-Count" };
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCustomMvc();
            services.AddSwaggerDocs();
            services.AddConsul();
            services.AddDefaultEdgeServices();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddJaeger();
            services.AddOpenTracing();
            //services.AddOcelot();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", cors =>
                        cors.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            //.AllowCredentials()
                            .WithExposedHeaders(Headers));
            });
            services.RegisterServiceForwarder<IEmployeeService>("employee-service");
            services.RegisterServiceForwarder<IEmployeeService>("operation-service");

            services.AddRabbitMq(new[] { typeof(Startup).Assembly });
            services.AddDispatchers();

            //services.AddControllers(setupAction => { setupAction.ReturnHttpNotAcceptable = true; });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStartupInitializer startupInitializer, IHostApplicationLifetime applicationLifetime, IConsulClient client)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseAllForwardedHeaders();
            //app.UseOcelot();
            //app.UseSwaggerDocs();
            app.UseErrorHandler();
            app.UseAuthentication();
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseServiceId();
            app.UseRabbitMq();

            var consulServiceId = app.UseConsul();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                client.Agent.ServiceDeregister(consulServiceId);
            });

            startupInitializer.InitializeAsync();
        }
    }
}
