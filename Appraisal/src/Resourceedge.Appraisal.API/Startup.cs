using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Resourceedge.Appraisal.API.Extensions;
using Resourceedge.Appraisal.Domain.DBContexts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Resourceedge.Appraisal.API.Services.CronJobServices;
using Resourceedge.Appraisal.API.Handlers;
using Resourceedge.Appraisal.API.Filters;
using Resourceedge.Appraisal.API.Services;
using Resourceedge.Email.Api.Interfaces;

namespace Resourceedge.Appraisal.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        private static readonly string[] Headers = new[] { "X-Operation", "X-Resource", "X-Total-Count", "X-Pagination" };

        public Startup(IConfiguration _config)
        {
            Configuration = _config;
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

            services.AddVersionedApiExplorer(
             options =>
             {
                 options.GroupNameFormat = "'v'VVV";
                 options.SubstituteApiVersionInUrl = true;
             });

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(2, 1);
            });
                     

            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", config =>
            {
                config.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Query.ContainsKey("access_token"))
                        {
                            context.Token = context.Request.Query["access_token"];
                        }
                        return Task.CompletedTask;
                    }
                };
                config.Authority = Configuration["Services:Authority"];
                config.Audience = "Appraisal";
                config.RequireHttpsMetadata = false;
            });
                       
            //services.AddHttpContextAccessor();

            services.AddTransient<IDbContext, EdgeAppraisalContext>(ctx => EdgeAppraisalContext.Create(
                Configuration.GetSection("DefaultConnection:ConnectionString").Value,
                Configuration.GetSection("DefaultConnection:DataBaseName").Value));
            services.AddTransient<IEmailSender, EmailSender>();

            services.RegisterHttpClient(Configuration);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.RegisterServices();        

            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }
            });
            services.AddSwaggerGen(c =>
            {
                var provider = services.BuildServiceProvider()
                       .GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerDoc(description.GroupName, new OpenApiInfo { Title = $"ResourceEdge Appraisal Api {description.ApiVersion}" });
                }

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.DocumentFilter<JsonPatchDocumentFilter>();
            });

            services.AddControllers(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                //setupAction.Conventions.Add(new GroupingByNamespaceConvention());
            })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
            .AddXmlDataContractSerializerFormatters();

            services.AddHostedService<DeactivateAppraisalService>();
            services.AddHostedService<ActivateAppraisalService>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler(appBuilder =>
                //{
                //    appBuilder.Run(async context =>
                //    {
                //        context.Response.StatusCode = 500;
                //        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                //    });
                //});

                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
            app.ConfigureExceptionHandler(env);
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                var provider = app.ApplicationServices
                       .GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/v{description.ApiVersion}/swagger.json", $"ReaourceEdge API v{description.ApiVersion}");
                }

                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
           //InitializerService.Seed(app);
        }
    }
}
