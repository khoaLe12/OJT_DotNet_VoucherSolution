using Base.Core.Application;
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
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class UserService : IUserService
{
    //private readonly ILogger<UserService> _logger;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork, ICurrentUserService currentUserService, RoleManager<Role> roleManager)
    {
        //_logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _roleManager = roleManager;
    }

    public async Task<UserManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<User> patchDoc, ModelStateDictionary ModelState)
    {
        var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
        var user = await _unitOfWork.Users.FindAsync(userId);
        var operations = patchDoc.Operations.Where(o => o.op != "replace" || o.path != "IsBlocked");

        if (!operations.IsNullOrEmpty())
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Hành động không được hỗ trợ"
            };
        }

        if(currentUser == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { $"Can not find user with the given id: {_currentUserService.UserId}" }
            };
        }

        if (user == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { $"Can not find user with the given id: {userId}" }
            };
        }

        // Check whether if current User is a manager of the updated User or not
        if (!user.PathFromRootManager.IsDescendantOf(currentUser.PathFromRootManager))
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = $"Bạn không phải là người quản lý của người dùng '{user.Name}'"
            };
        }

        Action<JsonPatchError> errorHandler = (error) =>
        {
            var operations = patchDoc.Operations.FirstOrDefault(op => op.path == error.AffectedObject.ToString());
            if (operations != null)
            {
                var propertyName = operations.path.Split('/').Last();
                ModelState.AddModelError(propertyName, error.ErrorMessage);
            }
            else
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }
        };

        patchDoc.ApplyTo(user, errorHandler);
        if (!ModelState.IsValid)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { ModelState.ToString() ?? $"Error when updating User '{user.Name}'" }
            };
        }

        if(await _unitOfWork.SaveChangesAsync())
        {
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }

        return new UserManagerResponse
        {
            IsSuccess = false,
            Message = "Cập nhật thất bại"
        };
    }

    public async Task<UserManagerResponse> UpdateInformation(UpdateInformationVM model)
    {
        try
        { 
            var existedUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (existedUser == null)
            {
                return new UserManagerResponse
                {
                    Message = "Không tìm thấy người dùng",
                    IsSuccess = false
                };
            }
            else
            {
                existedUser.Name = model.Name;
                existedUser.PhoneNumber = model.PhoneNumber;
                existedUser.Email = model.Email;
                existedUser.CitizenId = model.CitizenId;
            }

            if (await _unitOfWork.SaveChangesAsync())
            {
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công"
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại"
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public async Task<UserManagerResponse> ResetPassword(ResetPasswordVM model)
    {
        try
        {
            if (model is null)
            {
                return new UserManagerResponse
                {
                    Message = "Thông tin cập nhật không hợp lệ",
                    IsSuccess = false,
                };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return new UserManagerResponse
                {
                    Message = "Mật khẩu xác thực và mật khẩu không giống nhau",
                    IsSuccess = false,
                };
            }

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "Không tìm thấy người dùng",
                    IsSuccess = false
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật mật khẩu thành công"
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    Message = "Cập nhật mật khẩu thất bại",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật mật khẩu thất bại",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public async Task<UserManagerResponse> LoginUserAsync(LoginUserVM model)
    {
        var user = await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserName == model.UserName);
        if (user == null)
        {
            return new UserManagerResponse
            {
                Message = "Tài khoản hoặc mật khẩu sai",
                IsSuccess = false
            };
        }

        if (user.IsBlocked)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Tài khoản hiện đang bị khóa",
                Errors = new List<string>() { "Account is blocked" }
            };
        }

        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (result)
        {
            return new UserManagerResponse
            {
                Message = "Đăng nhập thành công",
                IsSuccess = true,
                LoginUser = user
            };
        }
        else
        {
            return new UserManagerResponse
            {
                Message = "Tài khoản hoặc mật khẩu sai",
                IsSuccess = false
            };
        }
    }

    public async Task<UserManagerResponse> RegisterNewUserAsync(UserVM model)
    {
        // Check Password
        if (model.Password != model.ConfirmPassword)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Mật khẩu xác thực và mật khẩu không giống nhau",
                Errors = new List<string>() { "Password and confirm password are not the same", $"{model.Password} - {model.ConfirmPassword}" }
            };
        }

        // Get list of User Role
        List<string>? roleNames = new();
        if (model.RoleIds.IsNullOrEmpty() || model.RoleIds == null)
        {
            roleNames = null;
        }
        else
        {
            foreach (var roleId in model.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy vai trò",
                        Errors = new List<string>() { "Can not find role with id: " + roleId }
                    };
                }
                roleNames.Add(role.NormalizedName);
            }
        }

        // Get Manager of new User
        HierarchyId? pathFromRootManager;
        if (model.ManagerId != null)
        {
            var manager = await _unitOfWork.Users.Get(u => u.Id == model.ManagerId, u => u.Roles!).FirstOrDefaultAsync();
            if (manager == null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người quản lý",
                };
            }

            var roles = manager.Roles;
            if (roles.IsNullOrEmpty() || roles == null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = $"'{manager.Name}' không phải là người quản lý"
                };
            }

            bool isManager = false;
            foreach (Role role in roles)
            {
                if (role.IsManager)
                {
                    isManager = true;
                    break;
                }
            }

            if (!isManager)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = $"'{manager.Name}' không phải là người quản lý"
                };
            }

            Expression<Func<User, bool>> where = u => u.PathFromRootManager.GetAncestor(1) == manager.PathFromRootManager;
            var maxNode = _unitOfWork.Users.Get(where).Max(u => u.PathFromRootManager);
            pathFromRootManager = manager.PathFromRootManager.GetDescendant(maxNode, null);
        }
        else
        {
            Expression<Func<User, bool>> where = u => u.PathFromRootManager.GetAncestor(1) == HierarchyId.GetRoot();
            var maxNode = _unitOfWork.Users.Get(where).Max(u => u.PathFromRootManager);
            pathFromRootManager = HierarchyId.GetRoot().GetDescendant(maxNode, null);
        }

        // Create new User and fill in informations
        var identityUser = new User
        {
            Name = model.Name ?? model.UserName,
            UserName = model.UserName,
            CitizenId = model.CitizenId,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = model.EmailConfirmed ?? false,
            PhoneNumberConfirmed = model.PhoneNumberConfirmed ?? false,
            TwoFactorEnabled = model.TwoFactorEnabled ?? false,
            IsBlocked = model.IsBlocked ?? false,
            PathFromRootManager = pathFromRootManager,
        };

        // Save changes
        var identityResult = await _userManager.CreateAsync(identityUser, model.Password);
        if (identityResult.Succeeded)
        {
            if(roleNames is not null)
            {
                var roleResult = await _userManager.AddToRolesAsync(identityUser, roleNames);
                if (!roleResult.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Tạo tài khoản thất bại",
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
                }
            }
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Tạo tài khoản thành công"
            };
        }
        else
        {
            return new UserManagerResponse
            {
                Message = "Tạo tài khoản thất bại",
                IsSuccess = false,
                Errors = identityResult.Errors.Select(e => e.Description)
            };
        }
    }

    public async Task<UserManagerResponse> AssignManager(Guid ManagerId, Guid UserId)
    {
        var newManager = await _unitOfWork.Users.Get(u => u.Id == ManagerId, u => u.Roles!).FirstOrDefaultAsync();
        var user = await _userManager.FindByIdAsync(UserId.ToString());
        
        if(newManager == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người quản lý",
                Errors = new List<string>() { "Manager not found with the given id: " + ManagerId }
            };
        }

        if(user == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { "User not found with the given id: " + UserId }
            };
        }

        // Check whether new manager is also old manager or not
        if (user.PathFromRootManager.GetAncestor(1) == newManager.PathFromRootManager)
        {
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Không có sự thay đổi",
                Errors = new List<string>() { $"'{user.Name}' is already managed by '{newManager.Name}'" }
            };
        }

        // Check roles
        if (newManager.Roles.IsNullOrEmpty() || newManager.Roles == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = $"'{newManager.Name ?? newManager.UserName}' không phải là người quản lý",
                Errors = new List<string>() { $"User '{newManager.Name ?? newManager.UserName}' do not have sufficient privileges to be a manager" }
            };
        }

        // Check whether if a manager or not
        bool isManager = false;
        var roles = newManager.Roles.Where(r => !r.IsDeleted); 
        foreach (Role r in newManager.Roles)
        {
            if (r.IsManager)
            {
                isManager = true;
                break;
            }
        }
        if (!isManager)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = $"'{newManager.Name ?? newManager.UserName}' không phải là người quản lý",
                Errors = new List<string>() { $"User '{newManager.Name ?? newManager.UserName}' do not have sufficient privileges to be a manager" }
            };
        }

        // Move nodes here
        await MoveNodes(user, newManager);

        // Save Changes
        if (await _unitOfWork.SaveChangesAsync())
        {
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { "Maybe Error from Server" }
            };
        }
    }

    public IEnumerable<User> GetAllUser()
    {
        return _unitOfWork.Users.Get(u => !u.IsDeleted, u => u.Roles!).AsNoTracking();
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _unitOfWork.Users
            .Get(u => !u.IsDeleted && u.Id == id, u => u.Roles!)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetAllManagedUser()
    {
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
        if(user == null)
        {
            throw new ArgumentNullException(null, "Người dùng không tồn tại");
        }
        Expression<Func<User, bool>> where = u =>
            !u.IsDeleted &&
            u.PathFromRootManager.IsDescendantOf(user.PathFromRootManager) && 
            u != user;
        return _unitOfWork.Users.Get(where);
    }

    public async Task<IEnumerable<User>> GetAllManager()
    {
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
        if (user == null)
        {
            throw new ArgumentNullException(null, "Người dùng không tồn tại");
        }
        Expression<Func<User, bool>> where = u => 
            !u.IsDeleted &&
            user.PathFromRootManager.IsDescendantOf(u.PathFromRootManager) && 
            u != user;
        return _unitOfWork.Users.Get(where);
    }

    public async Task<IEnumerable<User>> GetAllSupportingUser()
    {
        var customer = await _unitOfWork.Customers.FindAsync(_currentUserService.UserId);
        if (customer == null)
        {
            throw new ArgumentNullException(null, "Khách hàng không tồn tại");
        }
        return _unitOfWork.Users.Get(u => !u.IsDeleted && u.Customers!.Contains(customer));
    }

    public async Task<ServiceResponse> SoftDelete(Guid id)
    {
        var existedUser = await _unitOfWork.Users.Get(sp => sp.Id == id && !sp.IsDeleted).FirstOrDefaultAsync();
        if (existedUser == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Error = new List<string>() { "Can not find user with the given id: " + id }
            };
        }

        existedUser.IsDeleted = true;

        var log = new Log
        {
            Type = (int)AuditType.Delete,
            TableName = nameof(User),
            PrimaryKey = id.ToString()
        };

        await _unitOfWork.AuditLogs.AddAsync(log);

        if (await _unitOfWork.SaveChangesAsync())
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

    private async Task MoveNodes(User user, User manager)
    {
        // Get direct chidren of user
        var directChildren = await _unitOfWork.Users.Get(u => u.PathFromRootManager.GetAncestor(1) == user.PathFromRootManager).ToListAsync();

        // Get direct children of manager
        Expression<Func<User, bool>> where = u => u.PathFromRootManager.GetAncestor(1) == manager.PathFromRootManager;
        var childrenOfNewManager = await _unitOfWork.Users.Get(where).ToListAsync();

        // Check whether if manager has any managed user
        if (childrenOfNewManager != null)
        {
            var maxNode = childrenOfNewManager.Max(u => u.PathFromRootManager);
            user.PathFromRootManager = manager.PathFromRootManager.GetDescendant(maxNode, null);
        }
        else
        {
            user.PathFromRootManager = manager.PathFromRootManager.GetDescendant(null, null);
        }

        if (directChildren != null)
        {
            foreach (var child in directChildren)
            {
                await MoveNodes(child, user);
            }
        }
    }
}