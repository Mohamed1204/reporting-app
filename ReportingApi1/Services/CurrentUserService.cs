using ReportingApi1.Entities;
using System.Security.Claims;

namespace ReportingApi1.Services
{
    public interface ICurrentUserService
    {
        int CompanyId { get; }
        int UserId { get; }
        string UserName { get; }
        UserRole Role { get; }
        bool IsAdmin { get; }
        int ResolveCompanyId(int requestedCompanyId);
    }
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUserService(IHttpContextAccessor http)
        {
            _http = http;
        }
        public int CompanyId
        {
            get { return int.Parse(_http.HttpContext!.User.FindFirstValue("companyId")!); }
        }
        public int UserId
        {
            get { return int.Parse(_http.HttpContext!.User.FindFirstValue("userId")!); }
        }

        public string UserName
        {
            get { return _http.HttpContext!.User.FindFirstValue("userName")!; }
        }
        public UserRole Role
        {
            get
            {
                var roleStr = _http.HttpContext!.User.FindFirstValue(ClaimTypes.Role)!;
                return Enum.Parse<UserRole>(roleStr);
            }
        }
        public bool IsAdmin { get { return Role == UserRole.Admin; } }

        public int ResolveCompanyId(int requestedCompanyId)
        {
            if (IsAdmin) return requestedCompanyId;
            if (requestedCompanyId != CompanyId)
                throw new Exceptions.ForbiddenException("You can only operate on your own company's reports.");
            return requestedCompanyId;
        }
    }
}