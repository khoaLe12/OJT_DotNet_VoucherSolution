using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Base.API.Filter
{
    public class AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var user = context.GetHttpContext().User;
            var isAuthenticated = user.Identity?.IsAuthenticated; 
            if(isAuthenticated is true)
            {
                var permission = user.Claims.Where(c => c.Type == "scope");
                if (permission.Any(p => p.Value.Contains("Hangfire")))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
