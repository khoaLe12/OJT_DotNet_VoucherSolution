using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
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
        var existedVoucher = await _unitOfWork.Vouchers.FindAsync(voucherId);
        if (existedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found"
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
            };
        }

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Update Successfully"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Update Fail"
            };
        }
    }

    public async Task<ServiceResponse> UpdateVoucher(Voucher? updatedVoucher, int voucherId)
    {
        if (updatedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid: Update Information are null"
            };
        }

        var existedVoucher = await _unitOfWork.Vouchers.FindAsync(voucherId);
        if (existedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found"
            };
        }
        else
        {
            updatedVoucher.Id = voucherId;
            _unitOfWork.Vouchers.Update(updatedVoucher);

            if (await _unitOfWork.SaveChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Update successfully"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Fail"
                };
            }
        }
    }

    public async Task<Voucher?> AddNewVoucher(Voucher? voucher, Guid? CustomerId, int? VoucherTypeId)
    {
        try
        {
            if (voucher != null && CustomerId != null && VoucherTypeId != null)
            {
                var salesEmployee = await _unitOfWork.Users.FindAsync(_currentUserService.UserId);
                var customer = await _unitOfWork.Customers.FindAsync((Guid)CustomerId);
                var voucherType = await _unitOfWork.VoucherTypes.FindAsync((int)VoucherTypeId);

                if (salesEmployee != null && customer != null && voucherType != null)
                {
                    voucher.VoucherType = voucherType;
                    voucher.Customer = customer;
                    voucher.SalesEmployee = salesEmployee;

                    voucher.IssuedDate = DateTime.Now;
                    voucher.VoucherStatus = 2;

                    await _unitOfWork.Vouchers.AddAsync(voucher);
                    if (await _unitOfWork.SaveChangesAsync())
                    {
                        return voucher;
                    }
                }
            }
            return null;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public IEnumerable<Voucher>? GetAllVoucher()
    {
        return _unitOfWork.Vouchers.FindAll();
    }

    public async Task<IEnumerable<Voucher>?> GetAllVoucherOfUser()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, "User Not Found");
        }
        return await _unitOfWork.Vouchers.Get(b => b.SalesEmployeeId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Voucher>?> GetAllVoucherOfCustomer()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Customers.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, "Customer Not Found");
        }
        return await _unitOfWork.Vouchers.Get(b => b.CustomerId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<Voucher?> GetVoucherById(int id)
    {
        Expression<Func<Voucher, bool>> where = v => v.Id == id;
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
}
