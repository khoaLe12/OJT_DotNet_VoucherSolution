using Base.Core.Application;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Base.Infrastructure.Services;

internal class UserService : IUserService
{
    //private readonly ILogger<UserService> _logger;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoleService _roleService;

    public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork, ICurrentUserService currentUserService, RoleManager<Role> roleManager, IRoleService roleService)
    {
        //_logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _roleManager = roleManager;
        _roleService = roleService;
    }

    public async Task<UserManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<User> patchDoc, ModelStateDictionary ModelState)
    {
        var operation = patchDoc.Operations.Find(o => o.op == "replace" && o.path == "LockoutEnabled");
        if (patchDoc.Operations.Count() > 1 || operation == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Operation Not Supported"
            };
        }

        var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
        var user = await _unitOfWork.Users.FindAsync(userId);
        if(currentUser == null || user == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "User Not Found"
            };
        }

        // Check whether if current User is a manager of the updated User or not
        if (!user.PathFromRootManager.IsDescendantOf(currentUser.PathFromRootManager))
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = $"You are not manager of user '{user.Name}'"
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
                Message = ModelState.ToString()
            };
        }

        if(await _unitOfWork.SaveChangesAsync())
        {
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Update Successfully."
            };
        }

        return new UserManagerResponse
        {
            IsSuccess = false,
            Message = "Update fail"
        };
    }

    public async Task<UserManagerResponse> UpdateInformation(UpdateInformationVM model)
    {
        try
        {
            if (model is null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Invalid: Update Information are null"
                };
            }
            
            var existedUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (existedUser == null)
            {
                return new UserManagerResponse
                {
                    Message = "Can not found",
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
                    Message = "Update Information successfully."
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Update Information Fail"
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Update Information Fail",
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
                    Message = "Invalid: Credentials are null",
                    IsSuccess = false,
                };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return new UserManagerResponse
                {
                    Message = "Confirm password does not match the password",
                    IsSuccess = false,
                };
            }

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "Can not found",
                    IsSuccess = false
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Change Password Successfully."
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    Message = "Password has not been updated",
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
                Message = "Reset Password Fail",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public async Task<UserManagerResponse> LoginUserAsync(LoginUserVM model)
    {
        if (model == null)
        {
            return new UserManagerResponse
            {
                Message = "Invalid: Credentials are null",
                IsSuccess = false,
            };
        }

        var user = await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserName == model.UserName);
        if (user == null)
        {
            return new UserManagerResponse
            {
                Message = "User not found with the given User Name",
                IsSuccess = false
            };
        }

        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (result)
        {
            return new UserManagerResponse
            {
                Message = "Login Successfully",
                IsSuccess = true,
                LoginUser = user
            };
        }
        else
        {
            return new UserManagerResponse
            {
                Message = "Invalid Password",
                IsSuccess = false
            };
        }
    }

    public async Task<UserManagerResponse> RegisterNewUserAsync(UserVM model)
    {
        // Check Credentials
        if (model == null)
        {
            return new UserManagerResponse
            {
                Message = "Invalid: Credentials are null",
                IsSuccess = false,
            };
        }

        // Check Password
        if (model.Password != model.ConfirmPassword)
        {
            return new UserManagerResponse
            {
                Message = "Confirm password does not match the password",
                IsSuccess = false
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
                        Message = "Role Not Found",
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
                    Message = "Can not found Manager",
                };
            }

            var roles = manager.Roles;
            if (roles.IsNullOrEmpty() || roles == null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = $"User '{manager.Name}' don't have any role"
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
                    Message = $"User '{manager.Name}' is not a manager"
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
            LockoutEnd = model.LockoutEnd,
            LockoutEnabled = model.LockoutEnabled ?? false,
            EmailConfirmed = model.EmailConfirmed ?? false,
            PhoneNumberConfirmed = model.PhoneNumberConfirmed ?? false,
            TwoFactorEnabled = model.TwoFactorEnabled ?? false,
            PathFromRootManager = pathFromRootManager,
        };

        // Save changes
        var identityResult = await _userManager.CreateAsync(identityUser, model.Password);
        if (identityResult.Succeeded)
        {
            if(roleNames != null)
            {
                var roleResult = await _userManager.AddToRolesAsync(identityUser, roleNames);
                if (!roleResult.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "User has not been created",
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
                }
            }
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "User created successfully"
            };
        }
        else
        {
            return new UserManagerResponse
            {
                Message = "User has not been created",
                IsSuccess = false,
                Errors = identityResult.Errors.Select(e => e.Description)
            };
        }
    }

    public async Task<UserManagerResponse> AssignManager(Guid ManagerId, Guid UserId)
    {
        var newManager = await _unitOfWork.Users.Get(u => u.Id == ManagerId, u => u.Roles!).FirstOrDefaultAsync();
        var user = await _userManager.FindByIdAsync(UserId.ToString());
        
        if(newManager != null && user != null)
        {
            // Check whether new manager is also old manager or not
            if (user.PathFromRootManager.GetAncestor(1) == newManager.PathFromRootManager)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = $"User '{user.Name}' is already managed by '{newManager.Name}'"
                };
            }

            // Check roles
            if(newManager.Roles.IsNullOrEmpty() || newManager.Roles == null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = $"User '{newManager.Name}' are not a manager"
                };
            }

            // Check whether if a manager or not
            bool isManager = false;
            foreach(Role r in newManager.Roles)
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
                    Message = $"User '{newManager.Name}' are not a manager"
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
                    Message = "Update User Successfully"
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Update User Fail"
                };
            }
        }
        else
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "User or Manager Not Found"
            };
        }
    }

    public IEnumerable<User>? GetAllUser()
    {
        return _unitOfWork.Users.Get(u => true, u => u.Roles!).AsNoTracking();
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _unitOfWork.Users
            .Get(u => u.Id == id, u => u.Roles!)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>?> GetAllManagedUser()
    {
        try
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            Expression<Func<User, bool>> where = u => u.PathFromRootManager.IsDescendantOf(
                user.PathFromRootManager) && u != user;
            return _unitOfWork.Users.Get(where);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public async Task<IEnumerable<User>?> GetAllManager()
    {
        try
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            Expression<Func<User, bool>> where = u => user.PathFromRootManager.IsDescendantOf(u.PathFromRootManager) && u != user;
            return _unitOfWork.Users.Get(where);
        }
        catch (InvalidOperationException)
        {
            throw;
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