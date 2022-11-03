using System.Net;
using System.Threading.Tasks;
using Resourceedge.Email.Api.Model;

namespace Resourceedge.Email.Api.Interfaces
{
    public interface IEmailSender
    {
        Task<HttpStatusCode> SendToSingleEmployee(string Subject, SingleEmailDto singleEmail);
        Task<HttpStatusCode> SendToMultipleEmail(string subject, EmailDtoForMultiple emailDtos);
        Task<HttpStatusCode> SendMultipleEmail(string subject, string employeeName, EmailDtoForMultiple emailDtos, string message, string title);
        Task<string> FormatEmail(string Name, string supervisor, string employeeInitials, string reviewId);
        Task<string> FormatEmailLineManagerNewSubmission(string firstName, string employeeName, string employeeInitials, string appraiserName, string reviewId);
        Task<string> FormatEmail(string Name, string supervisor, string message);
        Task<string> FormatEmailAppraisalScore(string name, string score, string reviewName, string reviewId);
        Task<string> FormatEmailEpaAccept(string name);
        Task<string> FormatEmailEpaReject(string employeeName, string teamLeadName, string teamLeadInitials, string comment);
        Task<string> FormatEmailEmployeeReviewApproved(string employeeName, string hodName, string reviewId);
        Task<string> FormatEmailEmployeeReviewedByAppraiser(string employeeName, string appraiserName, string reviewId);
        Task<string> FormatEmailAppraiserReviewApproved(string appraiserFirstName, string hodName, string employeeName, string reviewName);

        Task<string> FormatEmailAppraiserReviewRejected(string appraiserFirstName, string employeeName, string employeeInitials, string hodName,
            string hodInitials, string reviewName, string reviewId, string comments);

        Task<string> FormatEmailEmployeeReviewDisabled(string employeeName, string reviewName);
        Task<string> FormatEmailEmployeeReviewEnabled(string employeeName, string reviewName, string durationStopDate, string periodStopDate);
    }
}
