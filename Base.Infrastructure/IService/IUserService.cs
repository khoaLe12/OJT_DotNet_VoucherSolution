

using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Base.Infrastructure.IService;

public interface IUserService
{
    Task<UserManagerResponse> RegisterNewUserAsync(UserVM model);
    Task<UserManagerResponse> LoginUserAsync(LoginUserVM model);
    Task<IEnumerable<Role>?> GetRolesByUserId(Guid id);
    IEnumerable<User>? GetAllUser();
    IEnumerable<User>? GetAllManagedUser();
    Task<User?> GetUserById(Guid id);
    Task<UserManagerResponse> ResetPassword(ResetPasswordVM model);
    Task<UserManagerResponse> UpdateInformation(UpdateInformationVM model);
    Task<UserManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<User> patchDoc, ModelStateDictionary ModelState);
}
