using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Grad.CsLib.Auth;

public static class AuthPolicy
{
    public const string DepartmentClaim = "department";
    public const string OverrideClaim = "override";
    
    public const string User = "User";
    public const string Admin = "Admin";
    public const string Auditor = "Auditor";
    public const string Department = "Department";
    
    /// <summary>
    /// Wildcard value for department claim to indicate access to all departments.
    /// </summary>
    public const string AnyDepartmentWildcard = "*";

    public static void AddAuthPolicy(AuthorizationOptions options)
    {
        options.AddPolicy(User, policy => policy.RequireRole("Department", "Admin", "Auditor"));
        options.AddPolicy(Admin, policy => policy.RequireRole("Admin"));
        options.AddPolicy(Auditor, policy => policy.Requirements.Add(new RoleOrOverrideRequirement("Auditor")));
        options.AddPolicy(Department, policy => policy.Requirements.Add(new RoleOrOverrideRequirement("Department")));
    }
}

public class RoleOrOverrideRequirement(string role) : IAuthorizationRequirement
{
    public string Role { get; } = role;
}

[RegisterService<IAuthorizationHandler>(LifeTime.Singleton)]
public class RoleOrOverrideHandler : AuthorizationHandler<RoleOrOverrideRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleOrOverrideRequirement requirement)
    {
        var overrideClaims = context.User.FindAll(AuthPolicy.OverrideClaim).Select(c => c.Value);
        
        // If there are any overrideClaims, use those. If no override claims are set at all,
        // continue to check roles
        var enumerable = overrideClaims.ToList();
        if (enumerable.Count != 0)
        {
            if (enumerable.Contains(requirement.Role))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
        if (context.User.IsInRole(requirement.Role))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
