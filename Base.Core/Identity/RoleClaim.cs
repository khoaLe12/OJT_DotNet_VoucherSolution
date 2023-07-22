using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Base.Core.Identity;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    public virtual Role? Role { get; set; }
    public override Guid RoleId { get; set; }
    public bool IsDeleted { get; set; } = false;
}
