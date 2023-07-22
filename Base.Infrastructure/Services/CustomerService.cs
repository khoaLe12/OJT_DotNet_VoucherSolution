using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
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
        var customer = await _unitOfWork.Customers.Get(c => !c.IsDeleted && c.Id == userId).FirstOrDefaultAsync();
        var operations = patchDoc.Operations.Where(o => o.op != "replace" || o.path != "IsBlocked");

        if (!operations.IsNullOrEmpty())
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Hành động không được hỗ trợ"
            };
        }

        if (customer == null)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy tài khoản"
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
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { ModelState.ToString() ?? $"Error when updating Customer '{customer.Name}'" }
            };
        }

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new CustomerManagerResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }

        return new CustomerManagerResponse
        {
            IsSuccess = false,
            Message = "Cập nhật thất bại"
        };
    }

    public async Task<CustomerManagerResponse> UpdateInformation(UpdateInformationVM model)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var existedCustomer = await _unitOfWork.Customers.Get(c => !c.IsDeleted && c.Id == userId).FirstOrDefaultAsync();
            if (existedCustomer == null)
            {
                return new CustomerManagerResponse
                {
                    Message = "Không tìm thấy tài khoản",
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
                    Message = "Cập nhật thành công"
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại"
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { ex.Message }
            };
        }
        catch(UniqueConstraintException ex)
        {
            List<string>? errorList = new List<string>() { ex.Message };
            if (ex.InnerException!.Message.Contains("IX_Customers_CitizenId"))
            {
                errorList.Add("Mã công dân đã tồn tại");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_Email"))
            {
                errorList.Add("Email đã tổn tại");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_PhoneNumber"))
            {
                errorList.Add("Số điện thoại đã tồn tại");
            }

            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Errors = errorList
            };
        }
    }

    public async Task<CustomerManagerResponse> ResetPassword(ResetPasswordVM model)
    {
        try
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                return new CustomerManagerResponse
                {
                    Message = "Mật khẩu xác thực và mật khẩu mới không giống nhau",
                    IsSuccess = false,
                };
            }

            var customer = await _customerManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if(customer == null)
            {
                return new CustomerManagerResponse
                {
                    Message = "Không tìm thấy tài khoản",
                    IsSuccess = false
                };
            }

            var result = await _customerManager.ChangePasswordAsync(customer, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công"
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Cập nhật thất bại",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public async Task<CustomerManagerResponse> LoginCustomerAsync(LoginCustomerVM model)
    {
        var loginInformation = model.AccountInformation;
        var customer = await _unitOfWork.Customers
            .Get(c => c.Email == loginInformation || c.PhoneNumber == loginInformation || c.CitizenId == loginInformation)
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return new CustomerManagerResponse
            {
                Message = "Tài khoản hoặc mật khẩu sai",
                IsSuccess = false
            };
        }

        if (customer.IsDeleted)
        {
            return new CustomerManagerResponse
            {
                Message = "Tài khoản hoặc mật khẩu sai",
                IsSuccess = false,
                Errors = new List<string>() { "Account has been deleted" }
            };
        }

        if (customer.IsBlocked)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Tài khoản hiện đang bị khóa",
                Errors = new List<string>() { "Account is blocked" }
            };
        }

        var result = await _customerManager.CheckPasswordAsync(customer, model.Password);
        if (result)
        {
            return new CustomerManagerResponse
            {
                Message = "Đăng nhập thành công",
                IsSuccess = true,
                LoginCustomer = customer
            };
        }
        else
        {
            return new CustomerManagerResponse
            {
                Message = "Tài khoản hoặc mật khẩu sai",
                IsSuccess = false
            };
        }
    }

    public async Task<CustomerManagerResponse> RegisterNewCustomerAsync(CustomerVM model)
    {
        try
        {
            if (model.CitizenId == null && model.Email == null && model.PhoneNumber == null)
            {
                return new CustomerManagerResponse
                {
                    Message = "Yêu cầu phải điền it nhất 1 trong 3 (CCCD, Email, Phone)",
                    IsSuccess = false
                };
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new CustomerManagerResponse
                {
                    Message = "Mật khẩu xác thực và mật khẩu không giống nhau",
                    IsSuccess = false
                };
            }

            var userList = new List<User>();
            if(model.SalesEmployeeIds is not null)
            {
                foreach (var userId in model.SalesEmployeeIds)
                {
                    var existedUser = await _unitOfWork.Users.Get(u => u.Id == userId && !u.IsDeleted).FirstOrDefaultAsync();
                    if(existedUser == null)
                    {
                        return new CustomerManagerResponse
                        {
                            IsSuccess = false,
                            Message = "Không tìm thấy người hỗ trợ",
                            Errors = new List<string>() { "Can not find user with the given id: " + userId }
                        };
                    }
                    userList.Add(existedUser);
                }
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
                EmailConfirmed = model.EmailConfirmed ?? false,
                PhoneNumberConfirmed = model.PhoneNumberConfirmed ?? false,
                TwoFactorEnabled = model.TwoFactorEnabled ?? false,
                IsBlocked = model.IsBlocked ?? false,
                SalesEmployees = userList
            };

            var result = await _customerManager.CreateAsync(identityCustomer, model.Password);
            if (result.Succeeded)
            {
                return new CustomerManagerResponse
                {
                    Message = "Tạo thành công",
                    IsSuccess = true
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    Message = "Tạo thất bại",
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
                errorList.Add("Mã công dân đã tồn tại");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_Email"))
            {
                errorList.Add("Email đã tổn tại");
            }
            if (ex.InnerException!.Message.Contains("IX_Customers_PhoneNumber"))
            {
                errorList.Add("Số điện thoại đã tồn tại");
            }

            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Tạo thất bại",
                Errors = errorList
            };
        }
        catch (ArgumentNullException ex)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Tạo thất bại",
                Errors = new List<string>() { ex.Message }
            };
        }
    }

    public IEnumerable<Customer> GetAllCustomers()
    {
        return _unitOfWork.Customers.FindAll().Where(c => !c.IsDeleted);
    }

    public async Task<IEnumerable<Customer>> GetAllSupportedCustomer()
    {
        var user = await _unitOfWork.Users.Get(u => u.Id == _currentUserService.UserId && !u.IsDeleted).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new ArgumentNullException(null, "Người dùng không tồn tại");
        }
        return _unitOfWork.Customers.Get(c => c.SalesEmployees!.Contains(user) && !c.IsDeleted);
    }

    public async Task<Customer?> GetCustomerById(Guid id)
    {
        return await _unitOfWork.Customers
            .Get(c => c.Id == id && !c.IsDeleted, new Expression<Func<Customer, object>>[]
            {
                c => c.Vouchers!,
                c => c.Bookings!
            })
            .Include(nameof(Customer.Bookings) + "." + nameof(Booking.SalesEmployee))
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResponse> SoftDelete(Guid id)
    {
        var existedCustomer = await _unitOfWork.Customers.Get(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync();
        if(existedCustomer == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy tài khoản",
                Error = new List<string>() { "Can not find customer with the given id: " + id }
            };
        }

        existedCustomer.IsDeleted = true;

        var log = new Log
        {
            Type = (int)AuditType.Delete,
            TableName = nameof(Customer),
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
}
