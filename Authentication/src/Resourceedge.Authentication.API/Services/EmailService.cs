using Resourceedge.Email.Api.Model;
using Resourceedge.Email.Api.Services;
using Resourceedge.Email.Api.SGridClient;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Resourceedge.Email.Api.Interfaces;
using System.Threading;
using Microsoft.AspNetCore.Hosting;

namespace Resourceedge.Authentication.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ISGClient client;
        private readonly IWebHostEnvironment _hostingEnvironment;
        EmailDispatcher dispatcher;
        public EmailService(ISGClient _client, EmailDispatcher _dispatcher, IWebHostEnvironment hostingEnvironment)
        {
            client = _client;
            dispatcher = _dispatcher;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<HttpStatusCode> SendToSingleEmployee(string Subject, SingleEmailDto singleEmail)
        {
            return await dispatcher.SendSingleEmail(Subject, singleEmail);
        }

        public async Task<string> FormatEmail(string Name, string Url)
        {
            try
            {
                var tempPath = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplate\\ResetTemplate.html");
                var fileInfo = new FileInfo(tempPath);
                if (fileInfo.Exists)
                {
                    var body = "";
                    using (StreamReader sr = new StreamReader(fileInfo.FullName))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                    body = body.Replace("{Username}", Name.Split(' ')[0]);
                    body = body.Replace("{Link}", Url);

                    return body;
                }
                return null;
            }
            catch (Exception ex)
            {
                //log exception here later
                return null;
            }

        }

        public async Task<string> FormatSuccessfulResetPasswordEmail(string name)
        {
            try
            {
                string body = "";
                
                var tempPath = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplate\\successfulResetPassword.html");
                var fileInfo = new FileInfo(tempPath);
               
                if (fileInfo.Exists)
                {
                    using (StreamReader sr = new StreamReader(fileInfo.FullName))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                    body = body.Replace("{firstName}", name);

                    return body;
                }
                return null;
            }
            catch (Exception ex)
            {
                //log exception here later
                return null;
            }

        }

        //public async Task<string> FormatEmailAppraisalScore(string Name, string score)
        //{
        //    string[] message = GetEmailMessage(score);
        //    string Url = "https://resourceedge.netlify.app/";
        //    try
        //    {
        //        string body = "";
        //        string filename = Path.GetFullPath("EmailTemplate\\emailTemplateScore.html");
        //        using (StreamReader sr = new StreamReader(filename))
        //        {
        //            body = await sr.ReadToEndAsync();
        //        }

        //        body = body.Replace("{FirstName}", Name);
        //        body = body.Replace("{Score}", score);
        //        body = body.Replace("{Message}", message[0]);
        //        body = body.Replace("{Status}", message[1]);
        //        body = body.Replace("{Url}", Url);

        //        return body;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        private string[] GetEmailMessage(string scores)
        {
            double score = Convert.ToDouble(scores);
            string[] message = new string[2];

            if (score >= 4.5)
            {
                message[1] = "Excellent";
                message[0] = "You did well, you achieved your targets and you have performed Excellently well. This is commendable and you are representing our values, Well done.";
            }
            else if (score < 4.5 && score >= 4)
            {
                message[1] = "Good";
                message[0] = "You did well, you achieved your targets and you have a good performance. This is commendable and you are representing our values, Well done.";
            }
            else if (score < 4 && score >= 3)
            {
                message[1] = "Average";
                message[0] = "You achieve average targets at all and you have performed well. This is acceptable and you can do better.";
            }
            else if (score < 3 && score >= 2)
            {
                message[1] = "Poor";
                message[0] = "You did not achieve your targets at all and you have performed poorly.This is totally unacceptable and doesn’t represent the values we hold dear.";
            }
            else if (score < 2)
            {
                message[1] = "Very Poor";
                message[0] = "You did not achieve your targets at all and you have performed poorly. This is totally unacceptable and doesn’t represent the values we hold dear.";
            }

            return message;
        }

    }



}
