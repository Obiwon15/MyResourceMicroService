using Resourceedge.Email.Api.Model;
using Resourceedge.Email.Api.Services;
using Resourceedge.Email.Api.SGridClient;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Email.Api.Interfaces;
using Microsoft.Extensions.Logging;

namespace Resourceedge.Appraisal.API.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ISGClient client;
        private readonly ILogger _logger;

        EmailDispatcher dispatcher;

        public EmailSender(ISGClient _client, EmailDispatcher _dispatcher, ILogger<EmailSender> logger)
        {
            client = _client;
            dispatcher = _dispatcher;
            _logger = logger;
        }

        public async Task<HttpStatusCode> SendToSingleEmployee(string Subject, SingleEmailDto singleEmail)
        {     
            return await dispatcher.SendSingleEmail(Subject, singleEmail);
        }

        public async Task<HttpStatusCode> SendToMultipleEmail(string subject, EmailDtoForMultiple emailDtos)
        {
            var emails = emailDtos.EmailObjects.Select(x => new EmailAddress(x.ReceiverEmailAddress, x.ReceiverFullName)).ToList();
            string textContent = emailDtos.PlainTextContent;
            string htmlContent = emailDtos.HtmlContent;

            return await dispatcher.SendSingleMailToMultipleEmail(subject, textContent, htmlContent, emails);

        }
        
        public async Task<HttpStatusCode> SendMultipleEmail(string subject, string employeeName, EmailDtoForMultiple emailDtos, string message, string title)
        {
            try
            {
                var emails = emailDtos.EmailObjects.Select(x => new EmailAddress(x.ReceiverEmailAddress, x.ReceiverFullName)).ToList();
                string url = "https://resourceedge.herokuapp.com";

                emails.Add(new EmailAddress("Nwabugwu.akomas@tenece.com ", "Nwabugwu Akomas"));
                
                foreach (var item in emails)
                {
                    SingleEmailDto singleEmail = new SingleEmailDto()
                    {
                        PlainTextContent = emailDtos.PlainTextContent,
                        HtmlContent = await FormatEmail(employeeName, item.Name, message),
                        ReceiverEmailAddress = item.Email,
                        ReceiverFullName = item.Name
                    };

                    if(singleEmail.HtmlContent == null)
                    {
                        singleEmail.HtmlContent = @$"<p>{employeeName} has added you as a supervisor, kindly login to resourceedge and approve his EPA.</p>";
                    }

                    await dispatcher.SendSingleEmail(subject, singleEmail);
                }
                return HttpStatusCode.OK;        
            }
            catch (Exception ex)
            {

                throw ex;
            }        
        }

        public async Task<string> FormatEmail(string Name, string supervisor, string employeeInitials, string reviewId)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/appraiserNewSubmission.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }

                body = body.Replace("{FullName}", Name);
                body = body.Replace("{employeeInitials}", employeeInitials);
                body = body.Replace("{firstName}", supervisor);
                body = body.Replace("{reviewId}", reviewId);

                return body;
            }
            catch(Exception ex)
            {
                return null;
            }
          
        }
        
        public async Task<string> FormatEmailLineManagerNewSubmission(string firstName, string employeeName, string employeeInitials, string appraiserName, string reviewId)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/LineManagerNewSubmission.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }

                body = body.Replace("{firstName}", firstName);
                body = body.Replace("{employeeName}", employeeName);
                body = body.Replace("{employeeInitials}", employeeInitials);
                body = body.Replace("{appraiserName}", appraiserName);
                body = body.Replace("{reviewId}", reviewId);

                return body;
            }
            catch(Exception ex)
            {
                return null;
            }
          
        }

        public async Task<string> FormatEmail(string Name, string supervisor, string message)
        {
            string Url = "https://resourceedge.herokuapp.com/";
            //var mailMessage = new MailMessage();
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/AppraisalNotification.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }

                body = body.Replace("{FullName}", Name);
                body = body.Replace("{GroupName}", "RESOURCE EDGE");
                body = body.Replace("{Supervisor}", supervisor);
                body = body.Replace("{Message}", message);
                //body = body.Replace("{Title}", title);
                body = body.Replace("{Link}", Url);

                return body;
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        public async Task<string> FormatEmailAppraisalScore(string name, string score, string reviewName, string reviewId)
        {
            string filename = GetEmailMessage(score);
            try
            {
                string body;
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                    sr.Close();
                }

                body = body.Replace("{firstname}", name);
                body = body.Replace("{score}", score);
                body = body.Replace("{reviewName}", reviewName);
                body = body.Replace("{reviewId}", reviewId);
                
                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string GetEmailMessage(string scores)
        {
            double score = Convert.ToDouble(scores);
            string filename = "";

            if (score >= 4.5)
            {
                filename = Path.GetFullPath("EmailTemplate/scoreTemplate5.html");
            }
            else if (score < 4.5 && score >= 4)
            {
                filename = Path.GetFullPath("EmailTemplate/scoreTemplate4to5.html");
            }
            else if (score < 4 && score >= 3)
            {
                filename = Path.GetFullPath("EmailTemplate/scoreTemplate3to4.html");
            }
            else if (score < 3 && score >= 2)
            {
                filename = Path.GetFullPath("EmailTemplate/scoreTemplate2to3.html");
            }
            else if (score < 2)
            {
                filename = Path.GetFullPath("EmailTemplate/scoreTemplate1to2.html");
            }

            return filename;
        }

        public async Task<string> FormatEmailEpaAccept(string name)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/employeeEpaApproved.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }
                
                body = body.Replace("{firstName}", name);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public async Task<string> FormatEmailEpaReject(string employeeName, string teamLeadName, string teamLeadInitials, string comment)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/employeeRejectedEPA.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }
                
                body = body.Replace("{firstName}", employeeName);
                body = body.Replace("{teamLeadInitials}", teamLeadInitials);
                body = body.Replace("{teamLeadName}", teamLeadName);
                body = body.Replace("{comment}", comment);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public async Task<string> FormatEmailEmployeeReviewApproved(string employeeName, string hodName, string reviewId)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/employeeReviewApproved.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }
                
                body = body.Replace("{firstName}", employeeName);
                body = body.Replace("{hodName}", hodName);
                body = body.Replace("{reviewId}", reviewId);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public async Task<string> FormatEmailEmployeeReviewedByAppraiser(string employeeName, string appraiserName, string reviewId)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/employeeReviewedByAppraiser.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }
                
                body = body.Replace("{firstName}", employeeName);
                body = body.Replace("{appraiserName}", appraiserName);
                body = body.Replace("{reviewId}", reviewId);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public async Task<string> FormatEmailAppraiserReviewApproved(string appraiserFirstName, string hodName, string employeeName, string reviewName)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/appraiserReviewApproved.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }
                
                body = body.Replace("{firstName}", appraiserFirstName);
                body = body.Replace("{hodName}", hodName);
                body = body.Replace("{employeeName}", employeeName);
                body = body.Replace("{reviewName}", reviewName);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> FormatEmailAppraiserReviewRejected(string appraiserFirstName, string employeeName, string employeeInitials, string hodName, 
            string hodInitials, string reviewName, string reviewId, string comments)
        {
            try
            {
                string body = "";
                string filename = Path.GetFullPath("EmailTemplate/appraiserReviewRejected.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }

                body = body.Replace("{firstName}", appraiserFirstName);
                body = body.Replace("{employeeName}", employeeName);
                body = body.Replace("{employeeInitials}", employeeInitials);
                body = body.Replace("{hodName}", hodName);
                body = body.Replace("{hodInitials}", hodInitials);
                body = body.Replace("{reviewId}", reviewId);
                body = body.Replace("{reviewName}", reviewName);
                body = body.Replace("{comments}", comments);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> FormatEmailEmployeeReviewDisabled(string employeeName, string reviewName)
        {
            try
            {
                string body;
                string filename = Path.GetFullPath("EmailTemplate/employeeReviewDisabled.html");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }

                body = body.Replace("{firstName}", employeeName);
                body = body.Replace("{reviewName}", reviewName);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public async Task<string> FormatEmailEmployeeReviewEnabled(string employeeName, string reviewName, string durationStopDate, string periodStopDate)
        {
            try
            {
                string body;
                string filename = Path.GetFullPath("EmailTemplate/employeeReviewStartTemplate.html");
                _logger.LogInformation("================ log filenames");
                _logger.LogInformation($"{filename}");
                _logger.LogInformation("================ log filenames");
                using (StreamReader sr = new StreamReader(filename))
                {
                    body = await sr.ReadToEndAsync();
                }

                body = body.Replace("{firstName}", employeeName);
                body = body.Replace("{reviewName}", reviewName);
                body = body.Replace("{durationStopDate}", durationStopDate);
                body = body.Replace("{periodStopDate}", periodStopDate);

                return body;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
