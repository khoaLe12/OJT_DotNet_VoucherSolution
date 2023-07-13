using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Services
{
    internal class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleService(RoleManager<Role> roleManager, UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> UpdateRoleClaims(IEnumerable<UpdateClaimVM> Claims, Guid roleId)
        {
            var role = await _unitOfWork.Roles.Get(r => r.Id == roleId, r => r.RoleClaims!).FirstOrDefaultAsync();
            if(role == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Role Not Found"
                };
            }

            var roleClaims = role.RoleClaims;
            foreach(var claim in Claims)
            {
                if(claim.Id != null)
                {
                    var roleClaim = roleClaims?.Where(rc => rc.Id == claim.Id).FirstOrDefault();
                    if (roleClaim == null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Some Permissions Not Found"
                        };
                    }
                    if(claim.ClaimValue == null)
                    {
                        //Remove roleClaim
                    }
                    roleClaim.ClaimValue = claim.ClaimValue;
                }
                else
                {
                    //Create new roleClaim
                }
            }
        }

        public async Task<ServiceResponse> PatchUpdate(Guid roleId, JsonPatchDocument<Role> patchDoc, ModelStateDictionary ModelState)
        {
            var notSupportedOperations = patchDoc.Operations.Where(o => o.op != "replace" || (o.path != "IsManager" && o.path != "Name"));
            if (!notSupportedOperations.IsNullOrEmpty())
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Operation Not Supported"
                };
            }

            var existedRole = await _roleManager.FindByIdAsync(roleId.ToString());
            if(existedRole == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Can Not Found Role"
                };
            }

            Action<JsonPatchError> errorHandler = (error) =>
            {
                var operation = patchDoc.Operations.FirstOrDefault(op => op.path == error.AffectedObject.ToString());
                if (operation != null)
                {
                    var propertyName = operation.path.Split('/').Last();
                    ModelState.AddModelError(propertyName, error.ErrorMessage);
                }
                else
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            };

            patchDoc.ApplyTo(existedRole, errorHandler);
            if (!ModelState.IsValid)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Fail",
                    Error = new List<string>() { ModelState.ToString() ?? "" }
                };
            }

            if (await _unitOfWork.SaveChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Update Successfully"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Fail"
                };
            }
        }

        public async Task<IEnumerable<Role>?> GetAllRole()
        {
            return await _unitOfWork.Roles.Get(r => true, r => r.RoleClaims!).ToListAsync();
        }

        public async Task<Role?> GetRoleById(Guid id)
        {
            return await _unitOfWork.Roles.Get(r => r.Id == id, r => r.RoleClaims!).FirstOrDefaultAsync();
        }

        public async Task<Role?> AddNewRole(RoleVM model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(null, "Role Information are null");
                }

                var identityRole = await _roleManager.FindByNameAsync(model.RoleName);
                if (identityRole != null)
                {
                    throw new ArgumentException("Role Name is already taken");
                }

                identityRole = new Role
                {
                    Name = model.RoleName!,
                    IsManager = model.IsManager,
                };

                var roleResult = await _roleManager.CreateAsync(identityRole);
                if (roleResult.Succeeded)
                {
                    if (model.Claims != null)
                    {
                        foreach (var claim in model.Claims)
                        {
                            // A claim is constructed by 2 parts, first is resource, second is actions on that resource
                            var resource = claim.Resource;
                            var actions = string.Join(" ", claim.Actions!);
                            await _roleManager.AddClaimAsync(identityRole, new Claim(CustomClaimTypes.Permission, string.Concat(resource, ":", actions)));
                        }
                    }
                    return identityRole;
                }
                return null;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Role>?> GetRolesByUserId(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                return await _unitOfWork.Roles.Get(r => r.Users!.Contains(user), r => r.RoleClaims!).ToListAsync();
                /*IEnumerable<string>? roleNames = await _userManager.GetRolesAsync(user);
                if (roleNames != null)
                {
                    List<Role> roles = new();
                    foreach (string roleName in roleNames)
                    {
                        var role = await _roleManager.FindByNameAsync(roleName);
                        roles.Add(role);
                    }
                    return roles;
                }*/
            }
            return null;
        }

        public async Task<IEnumerable<Claim>?> GetRoleClaimsOfUser(Guid id)
        {
            var roles = await GetRolesByUserId(id);
            if (roles != null)
            {
                List<Claim> claims = new();
                foreach (Role role in roles)
                {
                    if(role.RoleClaims != null)
                    {
                        claims.AddRange(role.RoleClaims.Select(rc => rc.ToClaim()));
                    }
                }
                return claims;
            }
            else
            {
                return null;
            }
        }
    }
}
