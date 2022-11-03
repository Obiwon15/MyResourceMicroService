using System.Net;
using System.Threading.Tasks;
using Resourceedge.Email.Api.Model;

namespace Resourceedge.Email.Api.Interfaces
{
    public interface IEmailService
    {
        Task<HttpStatusCode> SendToSingleEmployee(string Subject, SingleEmailDto singleEmail);
        Task<string> FormatEmail(string Name, string Url);
        Task<string> FormatSuccessfulResetPasswordEmail(string name);
    }
}