using Base.Core.Application;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BookingService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Booking?> AddNewBooking(Booking? booking, Guid? CustomerId, int? ServicePackageId, IEnumerable<int>? VoucherIds)
    {
        try
        {
            if (booking != null && CustomerId != null && ServicePackageId != null)
            {
                var salesEmployee = await _unitOfWork.Users.FindAsync(_currentUserService.UserId);
                var customer = await _unitOfWork.Customers.FindAsync((Guid)CustomerId);
                var servicePackage = await _unitOfWork.ServicePackages.FindAsync((int)ServicePackageId);

                if (VoucherIds != null)
                {
                    var voucherList = new List<Voucher>();
                    foreach (int id in VoucherIds)
                    {
                        var voucher = await _unitOfWork.Vouchers.FindAsync(id);
                        if (voucher != null)
                        {
                            voucherList.Add(voucher);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    booking.Vouchers = voucherList;
                }

                if (salesEmployee != null && customer != null && servicePackage != null)
                {
                    booking.SalesEmployee = salesEmployee;
                    booking.Customer = customer;
                    booking.ServicePackage = servicePackage;
                    booking.BookingDate = DateTime.Now;

                    await _unitOfWork.Bookings.AddAsync(booking);
                    if (await _unitOfWork.SaveChangesAsync())
                    {
                        return booking;
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

    public IEnumerable<Booking>? GetAllBookings()
    {
        try
        {
            return _unitOfWork.Bookings.FindAll();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Booking?> GetBookingById(int id)
    {
        Expression<Func<Booking, bool>> where = b => b.Id == id;
        Expression<Func<Booking, object>>[] includes = {
            b => b.Customer!,
            b => b.SalesEmployee!
        };
        return await _unitOfWork.Bookings.Get(where, includes)
            .Include(nameof(Booking.ServicePackage) + "." + nameof(ServicePackage.Services))
            .Include(nameof(Booking.Vouchers) + "." + nameof(Voucher.VoucherType))
            .FirstOrDefaultAsync();
    }
}
