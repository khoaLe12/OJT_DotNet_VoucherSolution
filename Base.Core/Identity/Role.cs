using Base.Core.Common;
using Base.Core.Entity;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace Base.Core.Identity;

public class Role : IdentityRole<Guid>, IAuditable
{
    public bool IsManager { get; set; } = false;
    public IEnumerable<User>? Users { get; set; }
    public virtual IEnumerable<RoleClaim>? RoleClaims { get; set; }
    public bool IsDeleted { get; set; } = false;
}
