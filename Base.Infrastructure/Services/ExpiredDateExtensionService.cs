using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Services;

internal class ExpiredDateExtensionService : IExpiredDateExtensionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ExpiredDateExtensionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /*public async Task<ServiceResponse> PatchUpdate(int id, JsonPatchDocument<ExpiredDateExtension> patchDoc, ModelStateDictionary ModelState)
    {
        var existedVoucherExtension = await _unitOfWork.ExpiredDateExtensions.FindAsync(id);
        
        if (existedVoucherExtension == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found Voucher Extension"
            };
        }

        var existedVoucher = await _unitOfWork.Vouchers.FindAsync(existedVoucherExtension.VoucherId);

        if (existedVoucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found Voucher"
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

        // Get Operation which update new expired date
        Microsoft.AspNetCore.JsonPatch.Operations.Operation? updateExpiredDateOperation = 
            patchDoc.Operations.Find(o => o.op == "replace" && o.path == "NewExpiredDate");
        // Check Value
        if (updateExpiredDateOperation != null)
        {
            var newDate = (DateTime)updateExpiredDateOperation.value;
            if (newDate < existedVoucherExtension.OldExpiredDate)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "New Expired Date should not be less than Old Expired Date"
                };
            }
        }

        patchDoc.ApplyTo(existedVoucherExtension, errorHandler);
        if (!ModelState.IsValid)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = ModelState.ToString(),
            };
        }

        // Check whether it could affect on Expired Date of Voucher
        if (existedVoucher.ExpiredDate == existedVoucherExtension.OldExpiredDate)
        {

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
    }*/

    public async Task<ServiceResponse> UpdateVoucherExtension(ExpiredDateExtension? updatedExpiredDateExtension, int id)
    {
        if (updatedExpiredDateExtension == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid: Update Information are null"
            };
        }

        var existedExpiredDateExtension = await _unitOfWork.ExpiredDateExtensions.FindAsync(id);
        var voucher = await _unitOfWork.Vouchers.FindAsync(updatedExpiredDateExtension.VoucherId);

        if (existedExpiredDateExtension == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found Voucher Extension"
            };
        }
        if (voucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found Voucher"
            };
        }
        if (existedExpiredDateExtension.VoucherId != updatedExpiredDateExtension.VoucherId)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Voucher is not allowed to be changed"
            };
        }
        if (_currentUserService.UserId != existedExpiredDateExtension.SalesEmployeeId)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "You can not update Voucher Extension that is not yours"
            };
        }

        updatedExpiredDateExtension.Id = id;
        updatedExpiredDateExtension.SalesEmployeeId = existedExpiredDateExtension.SalesEmployeeId;
        updatedExpiredDateExtension.DateTime = existedExpiredDateExtension.DateTime;
        updatedExpiredDateExtension.OldExpiredDate = existedExpiredDateExtension.OldExpiredDate;
        if (updatedExpiredDateExtension.OldExpiredDate > updatedExpiredDateExtension.NewExpiredDate)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Updated Expired Date should not be less than old Expired Date"
            };
        }
        if (voucher.ExpiredDate == existedExpiredDateExtension.NewExpiredDate)
        {
            voucher.ExpiredDate = updatedExpiredDateExtension.NewExpiredDate;
        }

        _unitOfWork.ExpiredDateExtensions.Update(updatedExpiredDateExtension);
        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Update Voucher Extension successfully"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Update Voucher Extension Fail"
            };
        }
    }

    public async Task<ExpiredDateExtension?> AddNewExpiredDateExtension(ExpiredDateExtension? expiredDateExtension, int? VoucherId)
    {
        try
        {
            if (expiredDateExtension != null && VoucherId != null)
            {
                var salesEmployee = await _unitOfWork.Users.FindAsync(_currentUserService.UserId);
                var voucher = await _unitOfWork.Vouchers.FindAsync((int)VoucherId);

                if (salesEmployee != null && voucher != null)
                {
                    if (voucher.ExpiredDate > expiredDateExtension.NewExpiredDate)
                    {
                        return null;
                    }
                    expiredDateExtension.OldExpiredDate = voucher.ExpiredDate;
                    expiredDateExtension.DateTime = DateTime.Now;
                    expiredDateExtension.SalesEmployee = salesEmployee;
                    expiredDateExtension.Voucher = voucher;

                    //Update new expired date of the Voucher
                    voucher.ExpiredDate = expiredDateExtension.NewExpiredDate;

                    await _unitOfWork.ExpiredDateExtensions.AddAsync(expiredDateExtension);
                    if (await _unitOfWork.SaveChangesAsync())
                    {
                        return expiredDateExtension;
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

    public IEnumerable<ExpiredDateExtension>? GetAllExpiredDateExtensions()
    {
        return _unitOfWork.ExpiredDateExtensions.FindAll();
    }

    public async Task<ExpiredDateExtension?> GetExpiredDateExtensionById(int id)
    {
        Expression<Func<ExpiredDateExtension, bool>> where = e => e.Id == id;
        Expression<Func<ExpiredDateExtension, object>>[] includes = {
            e => e.SalesEmployee!
        };
        return await _unitOfWork.ExpiredDateExtensions.Get(where, includes)
            .Include(nameof(ExpiredDateExtension.Voucher) + "." + nameof(Voucher.Customer))
            .Include(nameof(ExpiredDateExtension.Voucher) + "." + nameof(Voucher.VoucherType))
            .FirstOrDefaultAsync();
    }
}
