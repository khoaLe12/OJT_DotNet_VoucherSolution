using Base.Core.Common;
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
        //Task<ServiceResponse> UpdateRoleClaims(IEnumerable<UpdateClaimVM> Claims, Guid roleId);
        Task<Role?> AddNewRole(RoleVM model);
        Task<ServiceResponse> UpdateRole(Guid roleId, UpdatedRoleVM updatedRole);
        Task<ServiceResponse> AddRoleClaims(Guid roleId, IEnumerable<ClaimVM> claims);
        Task<ServiceResponse> UpdateRoleClaims(Guid roleId, IEnumerable<UpdatedClaimVM> claims);
        Task<IEnumerable<Role>> GetAllRole();
        Task<Role?> GetRoleById(Guid id);
        Task<IEnumerable<Claim>> GetRoleClaimsOfUser(Guid id);
        Task<ServiceResponse> SoftDeleteRole(Guid id);
        Task<ServiceResponse> SoftDeleteRoleClaim(int id);
    }
}
