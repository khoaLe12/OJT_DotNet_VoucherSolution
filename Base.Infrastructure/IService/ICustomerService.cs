using Base.Core.Common;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Base.Infrastructure.IService;

public interface ICustomerService
{
    Task<CustomerManagerResponse> ConfirmEmailAsync(Guid customerId, string token);
    Task<CustomerManagerResponse> LoginCustomerAsync(LoginCustomerVM model);
    Task<CustomerManagerResponse> RegisterNewCustomerAsync(CustomerVM model);
    IEnumerable<Customer> GetAllCustomers();
    IEnumerable<Customer> GetAllDeletedCustomers();
    Task<IEnumerable<Customer>> GetAllSupportedCustomer();
    Task<Customer?> GetCustomerById(Guid id);
    Task<CustomerManagerResponse> ResetPassword(ResetPasswordVM model);
    Task<CustomerManagerResponse> ForgetPasswordAsync(string email);
    Task<CustomerManagerResponse> ForgetAndResetPasswordAsync(ForgetPasswordVM model);
    Task<CustomerManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<Customer> patchDoc, ModelStateDictionary ModelState);
    Task<CustomerManagerResponse> UpdateInformation(UpdateInformationVM model);
    Task<CustomerManagerResponse> AssignSupporter(Guid customerId, IEnumerable<AssignSupporterVM> model);
    Task<ServiceResponse> SoftDelete(Guid id);
}
