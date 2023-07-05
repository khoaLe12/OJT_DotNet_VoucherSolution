using Base.Core.Application;
using Base.Core.Entity;
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
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        //_logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UserManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<User> patchDoc, ModelStateDictionary ModelState)
    {
        var currentUserId = _currentUserService.UserId;
        var roleListOfCurrentUser = await GetRolesByUserId(currentUserId);
        var user = await _unitOfWork.Users.FindAsync(userId);

        bool check = true;
        foreach (Role r in roleListOfCurrentUser!)
        {
            if(r.Name == "SupAdmin")
            {
                check = false;
            }
        }

        if (user == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Can not find User with the given Id !!!!"
            };
        }

        if (check)
        {
            if(user.ManagerId != currentUserId)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "You are not allowed to edit this user's information"
                };
            }
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
            Message = "Update fail at PatchUpdate method from UserService"
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
                    Message = "Invalid: Update Information are null !!!"
                };
            }
            
            var existedUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (existedUser == null)
            {
                return new UserManagerResponse
                {
                    Message = "User has not found with the given Id !!!",
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
                    Message = "Update Information Fail!!!"
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Update Information Fail!!!",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public async Task<IEnumerable<Role>?> GetRolesByUserId(Guid id)
    {
        var user = await _unitOfWork.Users.Get(u => u.Id == id, new Expression<Func<User, object>>[] {u => u.Roles!}).FirstOrDefaultAsync();
        if(user != null)
        {
            return user.Roles;
        }
        return null;
    }

    public async Task<UserManagerResponse> ResetPassword(ResetPasswordVM model)
    {
        try
        {
            if (model is null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Invalid: Credentials are null !!!"
                };
            }

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "User has not found with the given Id !!!",
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
                Message = "Reset Password Fail!!!",
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
                Message = "Invalid: Credentials are null !!!",
                IsSuccess = false,
            };
        }

        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            return new UserManagerResponse
            {
                Message = "User not found with the given User Name !!!",
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
                Message = "Invalid Password !!!",
                IsSuccess = false
            };
        }
    }

    public async Task<UserManagerResponse> RegisterNewUserAsync(UserVM model)
    {
        if (model == null)
        {
            return new UserManagerResponse
            {
                Message = "Invalid: Credentials are null !!!",
                IsSuccess = false,
            };
        }

        if (model.Password != model.ConfirmPassword)
        {
            return new UserManagerResponse
            {
                Message = "Confirm password does not match the password !!!",
                IsSuccess = false
            };
        }

        if (model.ManagerId != null)
        {
            var manager = await _unitOfWork.Users
                .Get(u => u.Id == (Guid)model.ManagerId, new Expression<Func<User, object>>[] { u => u.Roles! })
                .FirstOrDefaultAsync();
            if (manager == null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "User has not been created",
                    Errors = new List<string>() { "Manager Not Found with the given Id" }
                };
            }

            if (manager.Roles == null || !manager.Roles.Any(r => r.Name == "SupAdmin" || r.Name == "SalesAdmin"))
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "User has not been created",
                    Errors = new List<string>() { $"User '{manager.Name}' does not have a role or roles are not authorized to be a manager" }
                };
            }
        }

        List<Role>? roles = new();
        if (!model.RoleIds.IsNullOrEmpty())
        {
            foreach(var roleId in model.RoleIds)
            {
                var role = await _unitOfWork.Roles.FindAsync(roleId);
                if(role == null)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "User has not been created",
                        Errors = new List<string>() { "Role Not Found with the given Id" }
                    };
                }
                roles.Add(role);
            }
        }
        else
        {
            roles = null;
        }

        var identityUser = new User
        {
            Name = model.Name,
            UserName = model.UserName,
            CitizenId = model.CitizenId,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            LockoutEnd = model.LockoutEnd,
            LockoutEnabled = model.LockoutEnabled ?? false,
            EmailConfirmed = model.EmailConfirmed ?? false,
            PhoneNumberConfirmed = model.PhoneNumberConfirmed ?? false,
            TwoFactorEnabled = model.TwoFactorEnabled ?? false,
            ManagerId = model.ManagerId,
            Roles = roles
        };
        if(model.Name == null)
        {
            identityUser.Name = model.UserName;
        }

        var result = await _userManager.CreateAsync(identityUser, model.Password);
        if (result.Succeeded)
        {
            return new UserManagerResponse
            {
                Message = "User created successfully",
                IsSuccess = true
            };
        }

        return new UserManagerResponse
        {
            Message = "User has not been created",
            IsSuccess = false,
            Errors = result.Errors.Select(e => e.Description)
        };
    }

    public IEnumerable<User>? GetAllUser()
    {
        return _unitOfWork.Users.FindAll();
    }

    public IEnumerable<User>? GetAllManagedUser()
    {
        try
        {
            var userId = _currentUserService.UserId;
            return _unitOfWork.Users.Get(u => u.ManagerId == userId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _unitOfWork.Users
            .Get(u => u.Id == id)
            .Include(u => u.Customers)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync();
    }
}