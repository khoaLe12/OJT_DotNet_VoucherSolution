using Microsoft.AspNetCore.Authorization;

namespace Base.API.Permission
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
                return Task.CompletedTask;

            var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer)?.Value.Split(' ');
            
            if(scopes == null)
                return Task.CompletedTask;

            if (scopes.Any(s => requirement.Scope.Contains(s)))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
