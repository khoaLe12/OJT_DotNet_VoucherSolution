using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Data;
using System.Security.Claims;

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

        public async Task<Role?> AddNewRole(RoleVM model)
        {
            try
            {
                var identityRole = await _unitOfWork.Roles.Get(r => r.Name == model.RoleName).FirstOrDefaultAsync();
                if (identityRole != null)
                {
                    if(identityRole.IsDeleted == true)
                    {
                        throw new CustomException("Vai trò đã tồn tại")
                        {
                            Errors = new List<string>() { $"Role name '{model.RoleName}' already exists but has been deleted, you need to restored it" },
                            IsRestored = true
                        };
                    }

                    throw new CustomException("Vai trò đã tồn tại")
                    {
                        Errors = new List<string>() { $"Role name '{model.RoleName}' has already existed" }
                    };
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
                        var roleId = identityRole.Id;
                        var roleClaims = new ConcurrentBag<RoleClaim>();
                        model.Claims.AsParallel()
                            .WithDegreeOfParallelism(Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.2) * 2.0))
                            .ForAll(claim =>
                            {
                                RoleClaim r = new RoleClaim
                                {
                                    RoleId = roleId,
                                    ClaimType = CustomClaimTypes.Permission,
                                    ClaimValue = GetAction(claim)
                                };
                                roleClaims.Add(r);
                            });
                        identityRole.RoleClaims = roleClaims.ToList();

                        await _unitOfWork.SaveChangesAsync();
                    }
                    return identityRole;
                }
                else
                {
                    throw new CustomException("Tạo mới vai trò thất bại")
                    {
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
                }
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
        }

        public async Task<ServiceResponse> UpdateRole(Guid roleId, UpdatedRoleVM updatedRole)
        {
            var existedRole = await _roleManager.FindByIdAsync(roleId.ToString());
            if (existedRole == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy vai trò",
                    Error = new List<string>() { $"Can not find role with id: {roleId}" }
                };
            }

            if(existedRole.Name != updatedRole.RoleName)
            {
                var checkRole = await _roleManager.FindByNameAsync(updatedRole.RoleName);
                if (checkRole != null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Tên vai trò đã tồn tại",
                        Error = new List<string>() { $"Role name '{checkRole.Name}' has already existed" }
                    };
                }
            }

            existedRole.Name = updatedRole.RoleName;
            existedRole.IsManager = updatedRole.IsManager;

            var result = await _roleManager.UpdateAsync(existedRole);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại",
                    Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
                };
            }

        }

        public async Task<ServiceResponse> AddRoleClaims(Guid roleId, IEnumerable<ClaimVM> claims)
        {
            var existedRole = await _unitOfWork.Roles.Get(r => r.Id == roleId, r => r.RoleClaims!).FirstOrDefaultAsync();
            if (existedRole == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy vai trò"
                };
            }

            var existedRoleClaims = existedRole.RoleClaims;
            var exceptions = new ConcurrentQueue<Exception>();
            var addedClaims = new ConcurrentBag<RoleClaim>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.1) * 2))
            };
            Parallel.ForEach(claims, options, (claim, state) =>
            {
                var existedRoleClaim = existedRoleClaims?.Where(rc => rc.ClaimValue.Contains(claim.Resource!));
                if (!existedRoleClaim.IsNullOrEmpty())
                {
                    exceptions.Enqueue(new ArgumentException($"Resource '{claim.Resource}' already existed"));
                    state.Stop();
                    return;
                }

                RoleClaim r = new RoleClaim
                {
                    RoleId = roleId,
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = GetAction(claim)
                };
                addedClaims.Add(r);
            });

            if (!exceptions.IsNullOrEmpty())
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại",
                    Error = exceptions.Select(ex => ex.Message)
                };
            }

            if (addedClaims.IsNullOrEmpty())
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy thông tin cập nhật",
                    Error = new List<string>() { "Added Claims are null" }
                };
            }

            if (existedRole.RoleClaims.IsNullOrEmpty() || existedRole.RoleClaims == null)
            {
                existedRole.RoleClaims = addedClaims.ToList();
            }
            else
            {
                existedRole.RoleClaims = existedRole.RoleClaims.Concat(addedClaims).ToList();
            }

            if(await _unitOfWork.SaveChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại",
                    Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
                };
            }
        }

        public async Task<ServiceResponse> UpdateRoleClaims(Guid roleId, IEnumerable<UpdatedClaimVM> claims)
        {
            var existedRole = await _unitOfWork.Roles.Get(r => r.Id == roleId, r => r.RoleClaims!).FirstOrDefaultAsync();
            if (existedRole == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy vai trò"
                };
            }

            var existedRoleClaims = existedRole.RoleClaims;
            if (existedRoleClaims.IsNullOrEmpty() || existedRoleClaims == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không thể cập nhật quyền cho vai trò này",
                    Error = new List<string>() { $"Role {existedRole.Name} does not have any role claim" }
                };
            }

            var exceptions = new ConcurrentQueue<Exception>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.1 * 2))
            };
            Parallel.ForEach(claims, options, (claim, state) =>
            {
                var existedClaim = existedRoleClaims.FirstOrDefault(c => c.Id == claim.Id);
                if(existedClaim == null)
                {
                    exceptions.Enqueue(new ArgumentException($"Role Claim with id '{claim.Id}' Not Found"));
                    state.Stop();
                    return;
                }

                if (!existedClaim.ClaimValue.Contains(claim.Resource!))
                {
                    var existedRoleClaim = existedRoleClaims.FirstOrDefault(rc => rc.ClaimValue.Contains(claim.Resource!));
                    if (existedRoleClaim is not null)
                    {
                        exceptions.Enqueue(new ArgumentException($"Resource '{claim.Resource}' already existed"));
                        state.Stop();
                        return;
                    }
                }

                existedClaim.ClaimValue = GetAction(claim);
                _unitOfWork.RoleClaims.Update(existedClaim);
            });

            if (!exceptions.IsNullOrEmpty())
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại",
                    Error = exceptions.Select(ex => ex.Message)
                };
            }

            if(await _unitOfWork.SaveChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại",
                    Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
                };
            }
        }

        public async Task<IEnumerable<Role>> GetAllRole()
        {
            return await _unitOfWork.Roles.Get(r => !r.IsDeleted, r => r.RoleClaims!).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetAllDeletedRole()
        {
            return await _unitOfWork.Roles.Get(r => r.IsDeleted).AsNoTracking().ToListAsync();
        }

        public async Task<Role?> GetRoleById(Guid id)
        {
            return await _unitOfWork.Roles.Get(r => !r.IsDeleted && r.Id == id, r => r.RoleClaims!).FirstOrDefaultAsync();
        }

        public IEnumerable<Claim> GetRoleClaimsOfUser(User user)
        {
            var roles = user.Roles;
            if (roles != null)
            {
                ConcurrentDictionary<string, string> checkClaims = new ConcurrentDictionary<string,string>();
                foreach(Role role in roles)
                {
                    var roleClaims = role.RoleClaims;
                    roleClaims!.AsParallel()
                    .WithDegreeOfParallelism(Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.2 * 2)))
                    .ForAll(roleClaim =>
                    {
                        var claimValue = roleClaim.ClaimValue;
                        var resource = claimValue.Split(":").First().Trim();
                        var actions = claimValue.Split(":").Last().Trim();

                        if (!checkClaims.ContainsKey(resource))
                        {
                            checkClaims.TryAdd(resource, actions);
                        }
                        else
                        {
                            if (!resource.Equals(actions))
                            {
                                checkClaims[resource] = UpdateAction(checkClaims[resource], actions);
                            }
                        }
                    });
                }

                List<Claim> claims = new List<Claim>();
                foreach (var keyValue in checkClaims)
                {
                    claims.Add(new Claim("scope", string.Concat(keyValue.Key, ":", keyValue.Value)));
                }
                return claims;
            }
            else
            {
                return Enumerable.Empty<Claim>();
            }
        }

        private async Task<IEnumerable<Role>?> GetRolesByUserId(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                return await _unitOfWork.Roles.Get(r => !r.IsDeleted && r.Users!.Contains(user), r => r.RoleClaims!).ToListAsync();
            }
            return null;
        }

        public async Task<ServiceResponse> SoftDeleteRole(Guid id)
        {
            var existedRole = await _unitOfWork.Roles.Get(r => r.Id == id && !r.IsDeleted).FirstOrDefaultAsync();
            if(existedRole == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy vai trò",
                    Error = new List<string>() { "Can not find role with the given id: " + id }
                };
            }

            existedRole.IsDeleted = true;

            if (await _unitOfWork.SaveDeletedChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Xóa thành công"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Xóa thất bại",
                    Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
                };
            }
        }

        public async Task<ServiceResponse> DeleteRoleClaim(int id)
        {
            var existedRoleClaim = await _unitOfWork.RoleClaims.Get(rc => rc.Id == id).FirstOrDefaultAsync();
            if (existedRoleClaim == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy quyền",
                    Error = new List<string>() { "Can not find role claim with the given id: " + id }
                };
            }

            _unitOfWork.RoleClaims.Remove(existedRoleClaim);

            if (await _unitOfWork.SaveDeletedChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Xóa thành công"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Xóa thất bại",
                    Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
                };
            }
        }

        public async Task<ServiceResponse> RestoreRole(Guid id)
        {
            var deletedRole = await _unitOfWork.Roles.Get(b => b.Id == id && b.IsDeleted).FirstOrDefaultAsync();
            if (deletedRole is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy vai trò đã xóa",
                    Error = new List<string>() { "Can not find deleted role with the given id: " + id }
                };
            }
            deletedRole.IsDeleted = false;

            var log = await _unitOfWork.AuditLogs.Get(l => l.PrimaryKey == id.ToString() && l.Type == 3 && l.IsRestored != true && l.TableName == nameof(Role)).FirstOrDefaultAsync();
            if (log is not null)
            {
                log.IsRestored = true;
            }

            if (await _unitOfWork.SaveChangesNoLogAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Khôi phục thành công"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Khôi phục thất bại",
                    Error = new List<string>() { "Maybe there is error from server", "Maybe there is no change made" }
                };
            }
        }


        private string UpdateAction(string originalValue, string updateValue)
        {
            var actions = updateValue.Split(" ");
            foreach(var action in actions)
            {
                if (!originalValue.Contains(action))
                {
                    originalValue += new string(" " + action);
                }
            }
            return originalValue.Trim();
        }

        private string GetAction(ClaimVM model)
        {
            var actions = "";
            if (model.Read)
            {
                actions += "read ";
            }
            if (model.Write)
            {
                actions += "write ";
            }
            if (model.Update)
            {
                actions += "update ";
            }
            if (model.Delete)
            {
                actions += "delete ";
            }
            if (model.ReadAll)
            {
                actions += "all ";
            }
            if (model.Restore)
            {
                actions += "restore";
            }
            return string.Concat(model.Resource, ":", actions).Trim();
        }

        private string GetAction(UpdatedClaimVM model)
        {
            var actions = "";
            if (model.Read)
            {
                actions += "read ";
            }
            if (model.Write)
            {
                actions += "write ";
            }
            if (model.Update)
            {
                actions += "update ";
            }
            if (model.Delete)
            {
                actions += "delete ";
            }
            if (model.ReadAll)
            {
                actions += "all ";
            }
            if (model.Restore)
            {
                actions += "restore";
            }
            return string.Concat(model.Resource, ":", actions).Trim();
        }
    }
}
