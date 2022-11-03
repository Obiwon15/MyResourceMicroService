using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Resourceedge.Common.Mvc
{
    public static class Extensions
    {
        public static IServiceCollection AddIntializers(this IServiceCollection services, params Type[] initializiers)
        {
            if (initializiers is null) return services;

            return services.AddTransient<IStartupInitializer, StartupInitializer>(c =>
            {
                var startupInitializer = new StartupInitializer();
                var validInitializers = initializiers.Where(x => typeof(IInitializer).IsAssignableFrom(x));
                foreach (var initializer in validInitializers)
                {
                    startupInitializer.AddInitializer(c.GetService(initializer) as IInitializer);
                }
                return startupInitializer;
            });
        }

        public static IMvcCoreBuilder AddCustomMvc(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                services.Configure<AppOptions>(configuration.GetSection("app"));
            }

            services.AddSingleton<IStartupInitializer, StartupInitializer>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }
                config.EnableEndpointRouting = false;
            });

            return services.AddMvcCore()
                .AddDataAnnotations()
                .AddApiExplorer()
                .AddDefaultJsonOptions()
                .AddAuthorization();
        }

        public static IMvcCoreBuilder AddDefaultJsonOptions(this IMvcCoreBuilder builder)
        {

            return builder.AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                option.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                option.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                option.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                option.SerializerSettings.Formatting = Formatting.Indented;
                option.SerializerSettings.Converters.Add(new StringEnumConverter());
            }).AddXmlDataContractSerializerFormatters();
        }

        public static IApplicationBuilder UseAllForwardedHeaders(this IApplicationBuilder builder)
          => builder.UseForwardedHeaders(new ForwardedHeadersOptions
          {
              ForwardedHeaders = ForwardedHeaders.All
          });

        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
           => builder.UseMiddleware<ErrorHandlerMiddleware>();

        public static IApplicationBuilder UseServiceId(this IApplicationBuilder builder)
            => builder.Map("/id", c => c.Run(async ctx =>
            {
                using (var scope = c.ApplicationServices.CreateScope())
                {
                    var id = scope.ServiceProvider.GetService<IServiceId>().Id;
                    await ctx.Response.WriteAsync(id);
                }
            }));

        public static T Bind<T>(this T model, Expression<Func<T, object>> expression, object value)
           => model.Bind<T, object>(expression, value);

        public static T BindId<T>(this T model, Expression<Func<T, string>> expression)
            => model.Bind<T, string>(expression, ObjectId.GenerateNewId(DateTime.UtcNow).ToString());

        private static TModel Bind<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> expression,
            object value)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            var propertyName = memberExpression.Member.Name.ToLowerInvariant();
            var modelType = model.GetType();
            var field = modelType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith($"<{propertyName}>"));
            if (field == null)
            {
                return model;
            }

            field.SetValue(model, value);

            return model;
        }
    }


}
