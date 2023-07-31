using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.Text;
using Role = Base.Core.Identity.Role;

namespace Base.Infrastructure.Services;

internal class UserService : IUserService
{
    //private readonly ILogger<UserService> _logger;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly Cloudinary _cloudinary;

    public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork, ICurrentUserService currentUserService, RoleManager<Role> roleManager, IConfiguration configuration, Cloudinary cloudinary)
    {
        //_logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _roleManager = roleManager;
        _configuration = configuration;
        _cloudinary = cloudinary;
    }

    public async Task<UserManagerResponse> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if(user is null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Xác thực email thất bại",
                Errors = new List<string>() { "Can not find user with the given id: " + userId }
            };
        }

        var decodedToken = WebEncoders.Base64UrlDecode(token);
        string normalToken = Encoding.UTF8.GetString(decodedToken);

        // Make a custome email confirmation
        var isTokenValid = await _userManager.VerifyUserTokenAsync(user, "UserTokenProvider", UserManager<User>.ConfirmEmailTokenPurpose, normalToken);
        if (isTokenValid)
        {
            user.EmailConfirmed = true;
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Xác thực email thành công"
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Xác thực email thất bại",
                    Errors = new List<string>() { "Update fail" }
                };
            }
        }
        else
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Xác thực email thất bại",
                Errors = new List<string>() { "Invalid token." }
            };
        }

        // Use pre-defined ConfirmEmailAsync but need to force to use the given token provider
        /*_userManager.Options.Tokens.EmailConfirmationTokenProvider = "UserTokenProvider";
        var result = await _userManager.ConfirmEmailAsync(user, normalToken);
        if (result.Succeeded)
        {
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Xác thực email thành công"
            };
        }
        else
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Xác thực email thất bại",
                Errors = result.Errors.Select(e => e.Description)
            };
        }*/
    }

    public async Task<UserManagerResponse> UpdateRoleOfUser(Guid userId, IEnumerable<UpdatedRolesOfUserVM> model)
    {
        var user = await _unitOfWork.Users.Get(u => u.Id == userId, u => u.Roles!).FirstOrDefaultAsync();
        if (user == null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { $"Can not find user with the given id: {userId}" }
            };
        }

        var userRoles = user.Roles?.ToList();
        foreach (var updatedRole in model)
        {
            if (updatedRole.IsDeleted)
            {
                var existedRole = userRoles?.Where(r => r.Id == updatedRole.RoleId).FirstOrDefault();
                if(existedRole is null)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy vai trò",
                        Errors = new List<string>() { "Can not find role of user with the given id: " + updatedRole.RoleId }
                    };
                }
                userRoles!.Remove(existedRole);
            }
            else
            {
                var existedRole = await _unitOfWork.Roles.FindAsync(updatedRole.RoleId);
                if (existedRole is null)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy vai trò",
                        Errors = new List<string>() { "Can not find role with the given id: " + updatedRole.RoleId }
                    };
                }
                userRoles!.Add(existedRole);
            }
        }

        user.Roles = userRoles;

        if(await _unitOfWork.SaveChangesAsync())
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
                IsSuccess = true,
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { "May be there was no changes made", "Maybe error from server" }
            };
        }
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

            // If Email is updated then make a verification for it
            string? url = null;
            if (model.Email is not null && (existedUser.Email is null || !existedUser.Email.Equals(model.Email)))
            {
                if((await _userManager.FindByEmailAsync(model.Email)) is not null)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = $"Email '{model.Email}' đã tồn tại",
                        Errors = new List<string>() { $"Email '{model.Email}' has already existed" }
                    };
                }

                var confirmEmailtoken = await _userManager.GenerateUserTokenAsync(existedUser, "UserTokenProvider", UserManager<User>.ConfirmEmailTokenPurpose);

                var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailtoken);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                url = $"{_configuration["AppUrl"]}api/auth/confirmemail/user?userid={existedUser.Id}&token={validEmailToken}";
            }

            // Check if avatar is updated
            var file = model.Avatar;
            if (file is not null && file.Length > 0)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                // Check if there is error while uploading file
                if (uploadResult.Error != null)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Tải file ảnh thất bại",
                        Errors = new List<string> { uploadResult.Error.Message }
                    };
                }

                existedUser.FilePath = uploadResult.SecureUrl.ToString();
            }

            existedUser.Name = model.Name;
            existedUser.PhoneNumber = model.PhoneNumber;
            existedUser.Email = model.Email;
            existedUser.NormalizedEmail = model.Email!.ToUpper();
            existedUser.CitizenId = model.CitizenId;

            if(url is not null)
            {
                existedUser.EmailConfirmed = false;
            }

            if (await _unitOfWork.SaveChangesAsync())
            {
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công",
                    LoginUser = existedUser,
                    ConfirmEmailUrl = url
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

    public async Task<UserManagerResponse> ForgetPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if(user is null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { "No user associated with the given email: " + email }
            };
        }

        if (!user.EmailConfirmed)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Email chưa được xác thực",
                Errors = new List<string>() { "Email is not verified" }
            };
        }

        var token = await _userManager.GenerateUserTokenAsync(user, "UserTokenProvider", UserManager<User>.ResetPasswordTokenPurpose);
        var encodedToken = Encoding.UTF8.GetBytes(token);
        var validToken = WebEncoders.Base64UrlEncode(encodedToken);

        string url = $"{_configuration["AppUrl"]}resetpassword?email={email}&token={validToken}";

        return new UserManagerResponse
        {
            IsSuccess = true,
            Message = "Xác nhận thành công, vui lòng kiểm tra email",
            LoginUser = user,
            ConfirmEmailUrl = url,
        };
    }

    public async Task<UserManagerResponse> ForgetAndResetPasswordAsync(ForgetPasswordVM model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { "No user associated with the given email: " + model.Email }
            };
        }

        if(model.NewPassword != model.ConfirmPassword)
        {
            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Mật khẩu và mật khẩu xác nhận không giống nhau"
            };
        }

        var decodedToken = WebEncoders.Base64UrlDecode(model.Token!);
        string normalToken = Encoding.UTF8.GetString(decodedToken);

        // Force user manager to use the given token provider
        _userManager.Options.Tokens.PasswordResetTokenProvider = "UserTokenProvider";
        var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

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
                IsSuccess = false,
                Message = "Cập nhật mật khẩu thất bại",
                Errors = result.Errors.Select(e => e.Description)
            };
        }
    }

    public async Task<UserManagerResponse> LoginUserAsync(LoginUserVM model)
    {
        var user = await _unitOfWork.Users.Get(u => u.UserName == model.UserName) 
            .Include(nameof(User.Roles) + "." + nameof(Role.RoleClaims))
            .AsSplitQuery()
            .FirstOrDefaultAsync();

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

        if(model.Email is not null)
        {
            if ((await _userManager.FindByEmailAsync(model.Email)) is not null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = $"Email '{model.Email}' đã tồn tại",
                    Errors = new List<string>() { $"Email '{model.Email}' has already existed" }
                };
            }
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
            IsBlocked = model.IsBlocked ?? false,
            PathFromRootManager = pathFromRootManager,
        };

        //Upload file
        var file = model.Avatar;
        if (file is not null && file.Length > 0)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream())
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Check if there is error while uploading file
            if (uploadResult.Error != null)
            {
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Tải file ảnh thất bại",
                    Errors = new List<string> { uploadResult.Error.Message }
                };
            }

            identityUser.FilePath = uploadResult.SecureUrl.ToString();
        }

        // Save changes
        var identityResult = await _userManager.CreateAsync(identityUser, model.Password);
        if (identityResult.Succeeded)
        {
            string? url = null;
            if (!string.IsNullOrWhiteSpace(identityUser.Email))
            {
                var confirmEmailtoken = await _userManager.GenerateUserTokenAsync(identityUser, "UserTokenProvider", UserManager<User>.ConfirmEmailTokenPurpose);

                var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailtoken);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                url = $"{_configuration["AppUrl"]}api/auth/confirmemail/user?userid={identityUser.Id}&token={validEmailToken}";
            }
            if (roleNames is not null)
            {
                var roleResult = await _userManager.AddToRolesAsync(identityUser, roleNames);
                if (!roleResult.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Lỗi cấp quyền người dùng",
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
                }
            }
            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Tạo tài khoản thành công",
                LoginUser = identityUser,
                ConfirmEmailUrl = url,
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
        if (await _unitOfWork.SaveChangesNoLogAsync())
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

    public IEnumerable<User> GetAllDeletedUser()
    {
        return _unitOfWork.Users.Get(u => u.IsDeleted).AsNoTracking();
    }

    public async Task<User?> GetUserById(Guid id)
    {
        Expression<Func<User, object>>[] includes =
        {
            u => u.Roles!,
            u => u.Customers!
        };
        return await _unitOfWork.Users
            .Get(u => !u.IsDeleted && u.Id == id, includes)
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