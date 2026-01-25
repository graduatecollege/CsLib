using System.Security.Claims;

namespace Grad.CsLib.Auth;

public class UserInfo
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// The user's roles.
    /// </summary>
    public string[] Roles { get; set; }

    /// <summary>
    /// Roles the user has been overridden to have.
    /// </summary>
    public string[] RolesOverride { get; set; }

    /// <summary>
    /// Departments the user has access to.
    /// </summary>
    public string[] Departments { get; set; }

    /// <summary>
    /// Departments the user has been overridden to have access to.
    /// </summary>
    public string[] DepartmentsOverride { get; set; }

    public static UserInfo FromClaims(ClaimsPrincipal user)
    {
        return new UserInfo
        {
            Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            FirstName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty,
            LastName = user.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty,
            Roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray(),
            Departments = user.FindAll(AuthPolicy.DepartmentClaim).Select(d => d.Value).ToArray(),
            RolesOverride = user.FindAll(AuthPolicy.OverrideClaim).Select(r => r.Value).ToArray(),
            DepartmentsOverride = user.FindAll(AuthPolicy.DepartmentClaim).Select(d => d.Value).ToArray()
        };
    }

    public string Netid
    {
        get
        {
            var lowerEmail = Email.ToLowerInvariant();
            if (!lowerEmail.EndsWith("@illinois.edu") && !lowerEmail.EndsWith("@uillinois.edu"))
            {
                return string.Empty;
            }

            var atIndex = lowerEmail.IndexOf('@');
            return atIndex > 0 ? lowerEmail[..atIndex] : string.Empty;
        }
    }

    public string[] EffectiveRoles => RolesOverride.Length > 0 ? RolesOverride : Roles;
    public string[] EffectiveDepartments => DepartmentsOverride.Length > 0 ? DepartmentsOverride : Departments;

    public string ActiveRole
    {
        get
        {
            // Get first non-Admin role, or Admin if it's the only role they have
            var role = EffectiveRoles.FirstOrDefault(r => r != "Admin");

            if (role == null && EffectiveRoles.Contains("Admin"))
            {
                role = "Admin";
            }

            return role ?? string.Empty;
        }
    }

    public bool IsAuditorOrAdmin => EffectiveRoles.Contains("Auditor") || EffectiveRoles.Contains("Admin");
}