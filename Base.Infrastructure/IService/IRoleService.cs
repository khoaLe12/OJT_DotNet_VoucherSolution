using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService
{
    public interface IRoleService
    {
        Task<Role?> AddNewRole(RoleVM model);
        Task<ServiceResponse> UpdateRole(Guid roleId, UpdatedRoleVM updatedRole);
        Task<ServiceResponse> AddRoleClaims(Guid roleId, IEnumerable<ClaimVM> claims);
        Task<ServiceResponse> UpdateRoleClaims(Guid roleId, IEnumerable<UpdatedClaimVM> claims);
        Task<IEnumerable<Role>> GetAllRole();
        Task<IEnumerable<Role>> GetAllDeletedRole();
        Task<Role?> GetRoleById(Guid id);
        IEnumerable<Claim> GetRoleClaimsOfUser(User user);
        Task<ServiceResponse> SoftDeleteRole(Guid id);
        Task<ServiceResponse> DeleteRoleClaim(int id);
        Task<ServiceResponse> RestoreRole(Guid id);
    }
}
