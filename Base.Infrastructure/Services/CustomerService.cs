using Base.Core.Application;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class CustomerService : ICustomerService
{
    //private readonly ILogger<CustomerService> _logger;
    private readonly UserManager<Customer> _customerManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CustomerService(UserManager<Customer> customerManager, 
        //ILogger<CustomerService> logger, 
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
	{
        //_logger = logger;
        _customerManager = customerManager;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
	}

    public async Task<CustomerManagerResponse> PatchUpdate(Guid userId, JsonPatchDocument<Customer> patchDoc, ModelStateDictionary ModelState)
    {
        var customer = await _unitOfWork.Customers.FindAsync(userId);

        if (customer == null)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Can not find Customer with the given Id !!!!"
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

        patchDoc.ApplyTo(customer, errorHandler);
        if (!ModelState.IsValid)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = ModelState.ToString()
            };
        }

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new CustomerManagerResponse
            {
                IsSuccess = true,
                Message = "Update Successfully."
            };
        }

        return new CustomerManagerResponse
        {
            IsSuccess = false,
            Message = "Update fail at PatchUpdate method from UserService"
        };
    }

    public async Task<CustomerManagerResponse> UpdateInformation(UpdateInformationVM model)
    {
        try
        {
            if (model is null)
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Invalid: Update Information are null !!!"
                };
            }

            var existedCustomer = await _customerManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (existedCustomer == null)
            {
                return new CustomerManagerResponse
                {
                    Message = "Customer has not found with the given Id !!!",
                    IsSuccess = false
                };
            }
            else
            {
                existedCustomer.Name = model.Name;
                existedCustomer.PhoneNumber = model.PhoneNumber;
                existedCustomer.Email = model.Email;
                existedCustomer.NormalizedEmail = model.Email?.ToUpper();
                existedCustomer.CitizenId = model.CitizenId;
            }

            //Should catch possible exception when save
            if (await _unitOfWork.SaveChangesAsync())
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = true,
                    Message = "Update Information successfully."
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Update Information Fail!!!"
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Update Information Fail!!!",
                Errors = new List<string>() { ex.Message }
            };
        }
        catch(UniqueConstraintException ex)
        {
            List<string> errorList = new List<string>() { ex.Message };
            if (ex.InnerException!.Message.Contains("IX_Customers_CitizenId"))
            {
                errorList.Add("Citizen Id is already taken");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_Email"))
            {
                errorList.Add("Email is already taken");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_PhoneNumber"))
            {
                errorList.Add("Phone Number is already taken");
            }

            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Update Information fail.",
                Errors = errorList
            };
        }
    }

    public async Task<CustomerManagerResponse> ResetPassword(ResetPasswordVM model)
    {
        try
        {
            if(model == null)
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Invalid: Credentials are null !!!"
                };
            }

            var customer = await _customerManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if(customer == null)
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Customer has not found with the given Id !!!"
                };
            }

            var result = await _customerManager.ChangePasswordAsync(customer, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = true,
                    Message = "Password has been updated successfully."
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Password has not been updated !!!",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Reset Password Fail!!!",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public async Task<CustomerManagerResponse> LoginCustomerAsync(LoginCustomerVM model)
    {
        if (model == null)
        {
            return new CustomerManagerResponse
            {
                Message = "Invalid: Credentials are null !!!",
                IsSuccess = false,
            };
        }

        var customer = ((await _customerManager.FindByEmailAsync(model.AccountInformation))
            ?? (await _customerManager.Users.FirstOrDefaultAsync(c => c.PhoneNumber == model.AccountInformation || c.CitizenId == model.AccountInformation)));

        if (customer == null)
        {
            return new CustomerManagerResponse
            {
                Message = "Customer not found with the given information !!!",
                IsSuccess = false
            };
        }

        var result = await _customerManager.CheckPasswordAsync(customer, model.Password);
        if (result)
        {
            return new CustomerManagerResponse
            {
                Message = "Login Successfully",
                IsSuccess = true,
                LoginCustomer = customer
            };
        }
        else
        {
            return new CustomerManagerResponse
            {
                Message = "Invalid Password",
                IsSuccess = false
            };
        }
    }

    public async Task<CustomerManagerResponse> RegisterNewCustomerAsync(CustomerVM model)
    {
        try
        {
            if (model == null)
            {
                return new CustomerManagerResponse
                {
                    Message = "Invalid: Credentials are null !!!",
                    IsSuccess = false,
                };
            }

            if (model.CitizenId == null && model.Email == null && model.PhoneNumber == null)
            {
                return new CustomerManagerResponse
                {
                    Message = "At least one of the informations CitizenId, Email and Phone Number are required !!!",
                    IsSuccess = false
                };
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new CustomerManagerResponse
                {
                    Message = "Confirm password does not match the password !!!",
                    IsSuccess = false
                };
            }

            IEnumerable<User>? userList;
            if (model.SalesEmployeeIds != null)
            {
                userList = await _unitOfWork.Users.GetUsersById(model.SalesEmployeeIds.ToList());
            }
            else
            {
                userList = null;
            }

            var identityCustomer = new Customer
            {
                Name = model.Name,
                UserName = model.PhoneNumber ?? model.CitizenId ?? model.Email,
                CitizenId = model.CitizenId,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                LockoutEnd = model.LockoutEnd,
                LockoutEnabled = model.LockoutEnabled ?? false,
                EmailConfirmed = model.EmailConfirmed ?? false,
                PhoneNumberConfirmed = model.PhoneNumberConfirmed ?? false,
                TwoFactorEnabled = model.TwoFactorEnabled ?? false,
                SalesEmployees = userList
            };

            var result = await _customerManager.CreateAsync(identityCustomer, model.Password);
            if (result.Succeeded)
            {
                return new CustomerManagerResponse
                {
                    Message = "Customer created successfully",
                    IsSuccess = true
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    Message = "Customer has not been created",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }
        catch (UniqueConstraintException ex)
        {
            var errorList = new List<string>() { ex.Message };
            if (ex.InnerException!.Message.Contains("IX_Customers_CitizenId"))
            {
                errorList.Add("Citizen Id is already taken");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_Email"))
            {
                errorList.Add("Email is already taken");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_PhoneNumber"))
            {
                errorList.Add("Phone number is already taken");
            }

            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Customer has not been created",
                Errors = errorList
            };
        }
        catch (ArgumentNullException ex)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Customer has not been created",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public IEnumerable<Customer>? GetAllCustomers()
    {
        return _unitOfWork.Customers.FindAll();
    }

    public async Task<IEnumerable<Customer>?> GetAllSupportedCustomer()
    {
        try
        {
            var user = await _unitOfWork.Users.FindAsync(_currentUserService.UserId);
            if (user != null)
            {
                return _unitOfWork.Customers.Get(c => c.SalesEmployees!.Contains(user));
            }
            return null;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public async Task<Customer?> GetCustomerById(Guid id)
    {
        return await _unitOfWork.Customers
            .Get(c => c.Id == id, new Expression<Func<Customer, object>>[]
            {
                c => c.Vouchers!
            })
            .Include(nameof(Customer.Bookings) + "." + nameof(Booking.SalesEmployee))
            .FirstOrDefaultAsync();
    }
}
