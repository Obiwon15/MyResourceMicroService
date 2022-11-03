using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Resourceedge.Appraisal.Domain.Enums;
using Resourceedge.Appraisal.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Handlers
{
    public static class ExecptionHandler
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment hostingEnvironment)
        {

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    IExceptionHandlerFeature contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        //logger.LogError($"Something went wrong: {contextFeature.Error.Message}");
                        //logger.LogError($"Something went wrong: {contextFeature.Error.StackTrace}");


                        if (contextFeature.Error.GetType() == typeof(InvalidOperationException) ||
                            contextFeature.Error.GetType() == typeof(ArgumentException))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                            await context.Response.WriteAsync(new ErrorObject
                            {
                                status = ResponseStatus.APP_ERROR,
                                message = contextFeature.Error.Message,
                            }.ToString());
                           
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                            await context.Response.WriteAsync(new ErrorObject
                            {
                                status = ResponseStatus.FATAL_ERROR,

                                //message = "We currently cannot complete this request process. Please retry or contact our agent support network"
                                //message = contextFeature.Error.Message,

                                message = contextFeature.Error.Message
                                
                            }.ToString());
                        }
                    }
                });
            });
        }

    }
}
