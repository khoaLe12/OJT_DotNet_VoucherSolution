using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Base.Infrastructure.IService;

public interface IUserService
{
    Task<UserManagerResponse> ConfirmEmailAsync(Guid userId, string token);
    Task<UserManagerResponse> RegisterNewUserAsync(UserVM model);
    Task<UserManagerResponse> LoginUserAsync(LoginUserVM model);
    IEnumerable<User> GetAllUser();
    IEnumerable<User> GetAllDeletedUser();
    Task<IEnumerable<User>> GetAllManagedUser();
    Task<IEnumerable<User>> GetAllManager();
    Task<IEnumerable<User>> GetAllSupportingUser();
    Task<User?> GetUserById(Guid id);
    Task<UserManagerResponse> ResetPassword(ResetPasswordVM model);
    Task<UserManagerResponse> ForgetAndResetPasswordAsync(ForgetPasswordVM model);
    Task<UserManagerResponse> ForgetPasswordAsync(string email);
    Task<UserManagerResponse> AssignManager(Guid ManagerId, Guid UserId);
    Task<UserManagerResponse> UpdateInformation(UpdateInformationVM model);
    Task<UserManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<User> patchDoc, ModelStateDictionary ModelState);
    Task<UserManagerResponse> UpdateRoleOfUser(Guid userId, IEnumerable<UpdatedRolesOfUserVM> model);
    Task<ServiceResponse> SoftDelete(Guid id);
}
