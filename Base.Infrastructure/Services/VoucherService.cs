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

internal class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public VoucherService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
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
