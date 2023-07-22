using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Enum;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Base.Infrastructure.Migrations;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public VoucherService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResponse> PatchUpdate(int voucherId, JsonPatchDocument<Voucher> patchDoc, ModelStateDictionary ModelState)
    {
        var operations = patchDoc.Operations.Where(o => o.op != "replace" || o.path != "VoucherStatus");
        if (!operations.IsNullOrEmpty())
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Hành động không được hỗ trợ",
                Error = new List<string>() { "This action only support uppdate VoucherStatus of Voucher" }
            };
        }

        var existedVoucher = await _unitOfWork.Vouchers.FindAsync(voucherId);
        if (existedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy voucher",
                Error = new List<string> { $"Can not find voucher with the given id: {voucherId}" }
            };
        }

        var value = int.Parse(patchDoc.Operations.Where(o => o.path == "VoucherStatus").First().value.ToString() ?? "0");
        if (!Enum.IsDefined(typeof(VoucherStatus), value))
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Trạng thái của voucher không hợp lệ",
                Error = new List<string>() { $"The voucher status '{value}' does not exist" }
            };
        }

        Action<JsonPatchError> errorHandler = (error) =>
        {
            var operation = patchDoc.Operations.FirstOrDefault(op => op.path == error.AffectedObject.ToString());
            if (operation != null)
            {
                var propertyName = operation.path.Split('/').Last();
                ModelState.AddModelError(propertyName, error.ErrorMessage);
            }
            else
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }
        };

        patchDoc.ApplyTo(existedVoucher, errorHandler);
        if (!ModelState.IsValid)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = ModelState.ToString(),
                Error = new List<string>() { ModelState.ToString() ?? $"Error when updating Voucher with Id '{existedVoucher.Id}' "}
            };
        }

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
            };
        }
    }

    public async Task<ServiceResponse> UpdateVoucher(UpdatedVoucherVM updatedVoucher, int voucherId)
    {
        var existedVoucher = await _unitOfWork.Vouchers.FindAsync(voucherId);

        if (existedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy Voucher",
                Error = new List<string> { $"Can not find voucher with the given id: {voucherId}" }
            };
        }

        if (!Enum.IsDefined(typeof(VoucherStatus), updatedVoucher.VoucherStatus))
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Trạng thái của voucher không hợp lệ",
                Error = new List<string>() { $"The voucher status '{updatedVoucher.VoucherStatus}' does not exist" }
            };
        }

        existedVoucher.ExpiredDate = updatedVoucher.ExpiredDate;
        existedVoucher.ActualPrice = updatedVoucher.ActualPrice;
        existedVoucher.UsedValueDiscount = updatedVoucher.UsedValueDiscount;
        existedVoucher.VoucherStatus = updatedVoucher.VoucherStatus;

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
            };
        }
    }

    public async Task<Voucher?> AddNewVoucher(Voucher voucher, Guid CustomerId, int VoucherTypeId)
    {
        var salesEmployee = await _unitOfWork.Users.Get(u => u.Id == _currentUserService.UserId, u => u.Customers!).FirstOrDefaultAsync();
        var customer = await _unitOfWork.Customers.FindAsync(CustomerId);
        var voucherType = await _unitOfWork.VoucherTypes.FindAsync(VoucherTypeId);

        if (salesEmployee == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin người dùng:{_currentUserService.UserId}");
        }

        if (customer == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin khách hàng:{CustomerId}");
        }

        if(salesEmployee.Customers.IsNullOrEmpty() || !salesEmployee.Customers!.Contains(customer))
        {
            throw new CustomException($"Bạn không hỗ trợ khách hàng '{customer.Name ?? customer.UserName}'")
            {
                Errors = new List<string>() { $"User '{salesEmployee.Id}' does not support Customer '{customer.Name}'" }
            };
        }

        if (voucherType == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin loại voucher:{VoucherTypeId}");
        }

        if (!voucherType.IsAvailable)
        {
            throw new CustomException($"Loại voucher '{voucherType.TypeName}' không còn được sử dụng")
            {
                Errors = new List<string>() { $"Voucher type '{voucherType.TypeName}' is no more available" }
            };
        }

        if(voucherType.AvailableNumberOfVouchers == 0)
        {
            throw new CustomException($"Loại voucher '{voucherType.TypeName}' đã được bán hết") 
            { 
                Errors = new List<string>() { $"Voucher type '{voucherType.TypeName}' is out of stock" } 
            };
        }

        voucher.VoucherType = voucherType;
        voucher.Customer = customer;
        voucher.SalesEmployee = salesEmployee;

        voucher.IssuedDate = DateTime.Now;
        voucher.VoucherStatus = 2;

        await _unitOfWork.Vouchers.AddAsync(voucher);
        voucherType.AvailableNumberOfVouchers--;

        if (await _unitOfWork.SaveChangesAsync())
        {
            return voucher;
        }
        else
        {
            return null;
        }
    }

    public IEnumerable<Voucher> GetAllVoucher()
    {
        return _unitOfWork.Vouchers.FindAll().Where(v => !v.IsDeleted);
    }

    public async Task<IEnumerable<Voucher>> GetAllVoucherOfUser()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, "Không tìm thấy người dùng");
        }
        return await _unitOfWork.Vouchers.Get(v => !v.IsDeleted && v.SalesEmployeeId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Voucher>> GetAllVoucherOfCustomer()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Customers.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, "Không tìm thấy người dùng");
        }
        return await _unitOfWork.Vouchers.Get(v => !v.IsDeleted && v.CustomerId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<Voucher?> GetVoucherById(int id)
    {
        Expression<Func<Voucher, bool>> where = v => !v.IsDeleted && v.Id == id;
        Expression<Func<Voucher, object>>[] includes = {
            v => v.Customer!,
            v => v.SalesEmployee!,
            v => v.VoucherType!,
            v => v.Bookings!,
            v => v.VoucherExtensions!
        };
        return await _unitOfWork.Vouchers.Get(where, includes)
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResponse> SoftDelete(int id)
    {
        var existedVoucher = await _unitOfWork.Vouchers.Get(sp => sp.Id == id && !sp.IsDeleted).FirstOrDefaultAsync();
        if (existedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy voucher",
                Error = new List<string>() { "Can not find voucher with the given id: " + id }
            };
        }

        existedVoucher.IsDeleted = true;

        var log = new Log
        {
            Type = (int)AuditType.Delete,
            TableName = nameof(Voucher),
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
