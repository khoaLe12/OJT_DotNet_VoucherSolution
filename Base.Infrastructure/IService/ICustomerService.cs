using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Base.Infrastructure.IService;

public interface ICustomerService
{
    Task<CustomerManagerResponse> LoginCustomerAsync(LoginCustomerVM model);
    Task<CustomerManagerResponse> RegisterNewCustomerAsync(CustomerVM model);
    IEnumerable<Customer>? GetAllCustomers();
    Task<IEnumerable<Customer>?> GetAllSupportedCustomer();
    Task<Customer?> GetCustomerById(Guid id);
    Task<CustomerManagerResponse> ResetPassword(ResetPasswordVM model);
    Task<CustomerManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<Customer> patchDoc, ModelStateDictionary ModelState);
    Task<CustomerManagerResponse> UpdateInformation(UpdateInformationVM model);
}
