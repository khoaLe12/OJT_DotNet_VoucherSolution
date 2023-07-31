using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Duende.IdentityServer.Extensions;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.Text;

namespace Base.Infrastructure.Services;

internal class CustomerService : ICustomerService
{
    //private readonly ILogger<CustomerService> _logger;
    private readonly UserManager<Customer> _customerManager;
    private readonly IUserStore<Customer> _userStore;
    private readonly IUserEmailStore<Customer> _customerEmailStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly Cloudinary _cloudinary;

    public CustomerService(UserManager<Customer> customerManager, 
        IUserStore<Customer> userStore,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        Cloudinary cloudinary
        //ILogger<CustomerService> logger,
        )
    {
        //_logger = logger;
        _userStore = userStore;
        _customerManager = customerManager;
        _customerEmailStore = GetEmailStore();
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _cloudinary = cloudinary;
	}

    public async Task<CustomerManagerResponse> ConfirmEmailAsync(Guid customerId, string token)
    {
        var customer = await _customerManager.FindByIdAsync(customerId.ToString());
        if(customer is null)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Xác thực email thất bại",
                Errors = new List<string>() { "Can not find customer with the given id: " + customerId }
            };
        }

        var decodedToken = WebEncoders.Base64UrlDecode(token);
        string normalToken = Encoding.UTF8.GetString(decodedToken);

        // Make a customer check if token is valid or not
        var isTokenValid = await _customerManager.VerifyUserTokenAsync(customer, "CustomerTokenProvider", UserManager<Customer>.ConfirmEmailTokenPurpose, normalToken);
        if (isTokenValid)
        {
            customer.EmailConfirmed = true;
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = true,
                    Message = "Xác thực email thành công"
                };
            }
            else
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = false,
                    Message = "Xác thực email thất bại",
                    Errors = new List<string>() { "Update fail" }
                };
            }
        }
        else
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Xác thực email thất bại",
                Errors = new List<string>() { "Invalid token." }
            };
        }
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

            // If Email is updated then make a verification for it
            string? url = null;
            if (model.Email is not null && (existedCustomer.Email is null || !existedCustomer.Email.Equals(model.Email)))
            {
                if ((await _customerManager.FindByEmailAsync(model.Email)) is not null)
                {
                    return new CustomerManagerResponse
                    {
                        IsSuccess = false,
                        Message = $"Email '{model.Email}' đã tồn tại",
                        Errors = new List<string>() { $"Email '{model.Email}' has already existed" }
                    };
                }

                var confirmEmailtoken = await _customerManager.GenerateUserTokenAsync(existedCustomer, "CustomerTokenProvider", UserManager<Customer>.ConfirmEmailTokenPurpose);

                var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailtoken);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                url = $"{_configuration["AppUrl"]}api/auth/confirmemail/customer?customerid={existedCustomer.Id}&token={validEmailToken}";
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
                    return new CustomerManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Tải file ảnh thất bại",
                        Errors = new List<string> { uploadResult.Error.Message }
                    };
                }

                existedCustomer.FilePath = uploadResult.SecureUrl.ToString();
            }

            existedCustomer.Name = model.Name;
            existedCustomer.PhoneNumber = model.PhoneNumber;
            existedCustomer.Email = model.Email;
            existedCustomer.NormalizedEmail = model.Email?.ToUpper();
            existedCustomer.CitizenId = model.CitizenId;

            if(url is not null)
            {
                existedCustomer.EmailConfirmed = false;
            }

            //Should catch possible exception when save
            if (await _unitOfWork.SaveChangesAsync())
            {
                return new CustomerManagerResponse
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công",
                    LoginCustomer = existedCustomer,
                    ConfirmEmailUrl = url
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

    public async Task<CustomerManagerResponse> AssignSupporter(Guid customerId, IEnumerable<AssignSupporterVM> model)
    {
        var existedCustomer = await _unitOfWork.Customers.Get(c => c.Id == customerId, c => c.SalesEmployees!).FirstOrDefaultAsync();
        if(existedCustomer is null)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy khách hàng",
                Errors = new List<string>() { "Can not find customer with the given id: " + customerId }
            };
        }

        var supporters = existedCustomer.SalesEmployees!.ToHashSet();
        foreach (var item in model)
        {
            if (item.IsDeleted)
            {
                var checkExisted = supporters?.Where(u => u.Id == item.UserId).FirstOrDefault();
                if(checkExisted is null)
                {
                    return new CustomerManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng đang hỗ trợ",
                        Errors = new List<string>() { "Can not find supporting user with the given id: " + item.UserId }
                    };
                }
                supporters!.Remove(checkExisted);
            }
            else
            {
                var checkExisted = await _unitOfWork.Users.FindAsync(item.UserId);
                if (checkExisted is null)
                {
                    return new CustomerManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng",
                        Errors = new List<string>() { "Can not find user with the given id: " + item.UserId }
                    };
                }
                supporters.Add(checkExisted);
            }
        }

        // Make Updates
        existedCustomer.SalesEmployees = supporters;


        if (await _unitOfWork.SaveChangesNoLogAsync())
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
                Errors = new List<string>() { "Maybe there is no changes made", "Maybe error from server" }
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

    public async Task<CustomerManagerResponse> ForgetPasswordAsync(string email)
    {
        var user = await _customerManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { "No customer associated with the given email: " + email }
            };
        }

        if (!user.EmailConfirmed)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Email chưa được xác thực",
                Errors = new List<string>() { "Email is not verified" }
            };
        }

        var token = await _customerManager.GenerateUserTokenAsync(user, "CustomerTokenProvider", UserManager<Customer>.ResetPasswordTokenPurpose);
        var encodedToken = Encoding.UTF8.GetBytes(token);
        var validToken = WebEncoders.Base64UrlEncode(encodedToken);

        string url = $"{_configuration["AppUrl"]}resetpassword1?email={email}&token={validToken}";

        return new CustomerManagerResponse
        {
            IsSuccess = true,
            Message = "Xác nhận thành công, vui lòng kiểm tra email",
            LoginCustomer = user,
            ConfirmEmailUrl = url,
        };
    }

    public async Task<CustomerManagerResponse> ForgetAndResetPasswordAsync(ForgetPasswordVM model)
    {
        var user = await _customerManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Errors = new List<string>() { "No customer associated with the given email: " + model.Email }
            };
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Mật khẩu và mật khẩu xác nhận không giống nhau"
            };
        }

        var decodedToken = WebEncoders.Base64UrlDecode(model.Token!);
        string normalToken = Encoding.UTF8.GetString(decodedToken);

        // Force user manager to use the given token provider
        _customerManager.Options.Tokens.PasswordResetTokenProvider = "CustomerTokenProvider";
        var result = await _customerManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

        if (result.Succeeded)
        {
            return new CustomerManagerResponse
            {
                IsSuccess = true,
                Message = "Cập nhật mật khẩu thành công"
            };
        }
        else
        {
            return new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Cập nhật mật khẩu thất bại",
                Errors = result.Errors.Select(e => e.Description)
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

            if(model.Email is not null)
            {
                if ((await _customerManager.FindByEmailAsync(model.Email)) is not null)
                {
                    return new CustomerManagerResponse
                    {
                        IsSuccess = false,
                        Message = $"Email '{model.Email}' đã tồn tại",
                        Errors = new List<string>() { $"Email '{model.Email}' has already existed" }
                    };
                }
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
                IsBlocked = model.IsBlocked ?? false,
                SalesEmployees = userList
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
                    return new CustomerManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Tải file ảnh thất bại",
                        Errors = new List<string> { uploadResult.Error.Message }
                    };
                }

                identityCustomer.FilePath = uploadResult.SecureUrl.ToString();
            }


            var result = await _customerManager.CreateAsync(identityCustomer, model.Password);
            if (result.Succeeded)
            {
                string? url = null;
                if(!string.IsNullOrWhiteSpace(identityCustomer.Email))
                {
                    // Create email confirm token using custome token provider by passing its name
                    var confirmEmailToken = await _customerManager.GenerateUserTokenAsync(identityCustomer, "CustomerTokenProvider", UserManager<Customer>.ConfirmEmailTokenPurpose);

                    var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
                    var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                    url = $"{_configuration["AppUrl"]}api/auth/confirmemail/customer?customerid={identityCustomer.Id}&token={validEmailToken}";
                }

                return new CustomerManagerResponse
                {
                    Message = "Tạo tài khoản thành công",
                    IsSuccess = true,
                    LoginCustomer = identityCustomer,
                    ConfirmEmailUrl = url,
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

    public IEnumerable<Customer> GetAllDeletedCustomers()
    {
        return _unitOfWork.Customers.FindAll().Where(c => c.IsDeleted);
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

    private IUserEmailStore<Customer> GetEmailStore()
    {
        if (!_customerManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default email confirmation requires a user store with email support");
        }
        return (IUserEmailStore<Customer>)_userStore;
    }
}
