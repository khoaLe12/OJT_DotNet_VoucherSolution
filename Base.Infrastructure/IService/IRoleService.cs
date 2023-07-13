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
        Task<ServiceResponse> PatchUpdate(Guid roleId, JsonPatchDocument<Role> patchDoc, ModelStateDictionary ModelState);
        Task<Role?> AddNewRole(RoleVM model);
        Task<IEnumerable<Role>?> GetAllRole();
        Task<Role?> GetRoleById(Guid id);
        Task<IEnumerable<Role>?> GetRolesByUserId(Guid id);
        Task<IEnumerable<Claim>?> GetRoleClaimsOfUser(Guid id);
    }
}
