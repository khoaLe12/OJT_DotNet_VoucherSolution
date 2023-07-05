using Base.Core.Application;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
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
                    expiredDateExtension.ExtendedDateTime = DateTime.Now;
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
