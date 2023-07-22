

using Base.Core.Common;
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
    IEnumerable<User> GetAllUser();
    Task<IEnumerable<User>> GetAllManagedUser();
    Task<IEnumerable<User>> GetAllManager();
    Task<IEnumerable<User>> GetAllSupportingUser();
    Task<User?> GetUserById(Guid id);
    Task<UserManagerResponse> ResetPassword(ResetPasswordVM model);
    Task<UserManagerResponse> AssignManager(Guid ManagerId, Guid UserId);
    Task<UserManagerResponse> UpdateInformation(UpdateInformationVM model);
    Task<UserManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<User> patchDoc, ModelStateDictionary ModelState);
    Task<ServiceResponse> SoftDelete(Guid id);
}
