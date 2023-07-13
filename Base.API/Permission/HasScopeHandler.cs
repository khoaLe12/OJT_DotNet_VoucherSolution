using Microsoft.AspNetCore.Authorization;

namespace Base.API.Permission
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
                return Task.CompletedTask;

            var scopes = context.User.Claims.Where(claim => claim.Type == "scope" && claim.Issuer == requirement.Issuer);

            if(scopes == null)
                return Task.CompletedTask;

            foreach(var scope in scopes)
            {
                var requiredScope = requirement.Scope;
                var elements = scope.Value.Split(':');
                if (requiredScope.Contains(elements.First()))
                {
                    var actions = elements.Last().Split(' ');
                    if (actions.Any(a => requiredScope.Contains(a)))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
