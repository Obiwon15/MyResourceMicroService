using Microsoft.Extensions.DependencyInjection;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.API.Services;
using Resourceedge.Email.Api.Interfaces;
using Resourceedge.Email.Api.Services;
using Resourceedge.Worker.Auth.Services;

namespace Resourceedge.Appraisal.API.Extensions
{
    public static class MiddlewareExtension
    {
        public static void RegisterServices(this IServiceCollection services)
        {

            services.AddTransient<IServiceFactory, ServiceFactory>();
            services.AddTransient(typeof(EmailDispatcher));
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IKeyResultArea, KeyResultAreaService>();
            services.AddTransient<IAppraisalConfig, AppraisalConfigService>();
            services.AddTransient<IAppraisalResult, AppraisalResultService>();
            services.AddTransient<ITeamRepository, TeamService>();
            services.AddTransient<IAppraisalFinalResult, AppraisalFinalResultService>();
            services.AddTransient<ICoreValue, CoreValueService>();
            services.AddScoped<ITokenAccesor, TokenAccessorService>();
            services.AddTransient<ITeamLead, TeamLeadService>();
            services.AddTransient(typeof(AuthService));
        }
    }
}
