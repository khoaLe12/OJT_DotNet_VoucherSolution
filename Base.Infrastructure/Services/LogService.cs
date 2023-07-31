using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.EntityFrameworkCore;
using System.Collections.Specialized;
using System.Security.Claims;

namespace Base.Infrastructure.Services;

internal class LogService : ILogService
{
    private readonly IUnitOfWork _unitOfWork;

    public LogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Log>> GetUpdateActivities()
    {
        return await _unitOfWork.AuditLogs.Get(l => l.Type == (int)AuditType.Update).ToListAsync();
    }

    public async Task<IEnumerable<Log>> GetDeleteActivities()
    {
        return await _unitOfWork.AuditLogs.Get(l => l.Type == (int)AuditType.Delete).ToListAsync();
    }

    public async Task<IEnumerable<Log>> GetCreateActivities()
    {
        return await _unitOfWork.AuditLogs.Get(l => l.Type == (int)AuditType.Create).ToListAsync();
    }

    public async Task<Log?> GetLogById(int id)
    {
        return await _unitOfWork.AuditLogs.FindAsync(id);
    }

    public async Task<ServiceResponse> Recover(int id)
    {
        var existedLog = await _unitOfWork.AuditLogs.Get(l => l.Id == id && l.Type == (int)AuditType.Delete).FirstOrDefaultAsync();
        if(existedLog is null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy Log",
                Error = new List<string>() { "Can not find Log with the given Id: " + id, $"Or Log '{id}' is not a deleted record" }
            };
        }
        
        switch (existedLog.TableName)
        {
            case nameof(Booking):
                var existedBooking = await _unitOfWork.Bookings.FindAsync(int.Parse(existedLog.PrimaryKey!));
                if(existedBooking == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find booking with the given id: " + existedLog.PrimaryKey, "Booking does not exist in the system" }
                    };
                }
                existedBooking.IsDeleted = false;
                break;

            case nameof(Role):
                var existedRole = await _unitOfWork.Roles.FindAsync(Guid.Parse(existedLog.PrimaryKey!));
                if (existedRole == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find role with the given id: " + existedLog.PrimaryKey, "Role does not exist in the system" }
                    };
                }
                existedRole.IsDeleted = false;
                break;

            case nameof(RoleClaim):
                var changes = existedLog.Changes;
                if(changes is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find recovery information" }
                    };
                }

                var checkRole = await _unitOfWork.Roles
                    .Get(r => r.Id == Guid.Parse((string)changes["RoleId"]) && !r.IsDeleted, r => r.RoleClaims!)
                    .FirstOrDefaultAsync();

                var claimType = (string)changes["ClaimType"];
                var claimValue = (string)changes["ClaimValue"];

                if (checkRole is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find role with the given id: " + changes["RoleId"] }
                    };
                }

                var resource = claimValue.Split(":").First();

                if (checkRole.RoleClaims!.Any(rc => rc.ClaimType == claimType && rc.ClaimValue.Contains(resource)))
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { $"Resource '{resource}' already existed" }
                    };
                }

                var restoredRoleClaim = new RoleClaim
                {
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    Role = checkRole
                };
                await _unitOfWork.RoleClaims.AddAsync(restoredRoleClaim);
                break;

            case nameof(ServicePackage):
                var existedServicePackage = await _unitOfWork.ServicePackages.FindAsync(int.Parse(existedLog.PrimaryKey!));
                if (existedServicePackage == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find service package with the given id: " + existedLog.PrimaryKey , "Service package does not exist in the system"}
                    };
                }
                existedServicePackage.IsDeleted = false;
                break;

            case nameof(Service):
                var existedService = await _unitOfWork.Services.FindAsync(int.Parse(existedLog.PrimaryKey!));
                if (existedService == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find service with the given id: " + existedLog.PrimaryKey, "Service does not exist in the system" }
                    };
                }
                existedService.IsDeleted = false;
                break;

            case nameof(Voucher):
                var existedVoucher = await _unitOfWork.Vouchers.FindAsync(int.Parse(existedLog.PrimaryKey!));
                if (existedVoucher == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find voucher with the given id: " + existedLog.PrimaryKey, "Voucher does not exist in the system" }
                    };
                }
                existedVoucher.IsDeleted = false;
                break;

            case nameof(VoucherType):
                var existedVoucherType = await _unitOfWork.VoucherTypes.FindAsync(int.Parse(existedLog.PrimaryKey!));
                if (existedVoucherType == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find voucher type with the given id: " + existedLog.PrimaryKey, "Voucher type does not exist in the system" }
                    };
                }
                existedVoucherType.IsDeleted = false;
                break;

            case nameof(Customer):
                var existedCustomer = await _unitOfWork.Customers.FindAsync(Guid.Parse(existedLog.PrimaryKey!));
                if (existedCustomer == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find customer with the given id: " + existedLog.PrimaryKey, "Customer does not exist in the system" }
                    };
                }
                existedCustomer.IsDeleted = false;
                break;

            case nameof(User):
                var existedUser = await _unitOfWork.Users.FindAsync(Guid.Parse(existedLog.PrimaryKey!));
                if (existedUser == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find user with the given id: " + existedLog.PrimaryKey, "User does not exist in the system" }
                    };
                }
                existedUser.IsDeleted = false;
                break;

            case nameof(ExpiredDateExtension):
                var existedVoucherExtension = await _unitOfWork.ExpiredDateExtensions.FindAsync(int.Parse(existedLog.PrimaryKey!));
                if (existedVoucherExtension == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Khôi phục thất bại",
                        Error = new List<string>() { "Can not find voucher extension with the given id: " + existedLog.PrimaryKey, "Voucher extension does not exist in the system" }
                    };
                }
                existedVoucherExtension.IsDeleted = false;
                break;
            default:
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Khôi phục thất bại",
                    Error = new List<string>() { "Can not find entity named: " + existedLog.TableName }
                };
        }

        if(await _unitOfWork.SaveChangesNoLogAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Khôi phục thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Khôi phục thất bại",
                Error = new List<string>() { "Maybe no changes made", "Maybe existed entity is not deleted", "Maybe existed entity is already restored", "Maybe error from server" }
            };
        }
    }
}
