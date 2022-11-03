using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Resourceedge.Common;
using Resourceedge.Common.Consul;
using Resourceedge.Common.Dispatchers;
using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.Jaeger;
using Resourceedge.Common.Mongo;
using Resourceedge.Common.Mongo.Extensions;
using Resourceedge.Common.Mvc;
using Resourceedge.Common.RabbitMq;
using Resourceedge.Common.RestEase;
using Resourceedge.Employee.API.Configurations;
using Resourceedge.Employee.API.Services;
using Resourceedge.Employee.API.Services.HttpServices;
using Resourceedge.Employee.Application.Commands;
using Resourceedge.Employee.Application.Handlers;
using Resourceedge.Employee.Domain.DbContext;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Interfaces;
using Resourceedge.Employee.Infrastructure.Repositories;

namespace Resourceedge.Employee.API
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        private static readonly string[] Headers = new[] { "X-Operation", "X-Resource", "X-Total-Count", "X-Pagination" };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(config =>
                {
                    config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders(Headers);
                });
            });

            services.AddCustomMvc();
            services.AddCustomHttpClients(Headers);
            services.AddDefaultEdgeServices();
            services.AddTransient<IDbContext, EmployeeDbContext>(ctx => EmployeeDbContext.Create(
               Configuration.GetSection("DefaultConnection:ConnectionString").Value,
               Configuration.GetSection("DefaultConnection:DataBaseName").Value));
            services.AddTransient<IOldEmployee, ArchiveServices>();
            services.AddTransient<IEmployee, EmployeeBioDataService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers(setupAction => {setupAction.ReturnHttpNotAcceptable = true;});

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ResourceEdge Employee Api" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddDispatchers();
            services.AddConsul();
            services.AddJaeger();
            services.AddOpenTracing();

            services.RegisterServiceForwarder<ITestHttpService>("appraisal-service");

            services.AddIntializers(typeof(MongoDbInitializer));
            services.AddMongo();
           // services.AddRabbitMq(new[] { typeof(Startup).Assembly });
           //services.AddMongoRepository<Person>("Persons");
            //services.AddTransient<IPersonRepository, PersonRepository>();
            //services.AddTransient<ICommandHandler<CreatePerson>, CreatePersonHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStartupInitializer initializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            IdentityModelEventSource.ShowPII = true;
            app.UseCors();
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resource Edge Employee");
                c.RoutePrefix = string.Empty;

            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //app.UseRabbitMq()
            //    .SubscribeCommand<CreatePerson>();
            //initializer.InitializeAsync();

            //Initializer.SeedEmployeeDb(app);
        }
    }
}
