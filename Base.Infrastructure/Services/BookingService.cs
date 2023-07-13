using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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

    public async Task<ServiceResponse> PatchUpdate(int bookingId, JsonPatchDocument<Booking> patchDoc, ModelStateDictionary ModelState)
    {
        var existedBooking = await _unitOfWork.Bookings.FindAsync(bookingId);
        if(existedBooking == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found Booking"
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

        patchDoc.ApplyTo(existedBooking, errorHandler);
        if (!ModelState.IsValid)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = ModelState.ToString(),
            };
        }

        if(await _unitOfWork.SaveChangesAsync())
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

    public async Task<ServiceResponse> UpdateBooking(Booking? updatedBooking, int bookingId)
    {
        if(updatedBooking == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid: Update Information are null"
            };
        }

        var existedBooking = await _unitOfWork.Bookings.FindAsync(bookingId);
        if(existedBooking == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found Booking"
            };
        }

        if (_currentUserService.UserId != existedBooking.SalesEmployeeId)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "You can not update booking that is not yours"
            };
        }

        updatedBooking.Id = bookingId;
        updatedBooking.SalesEmployeeId = existedBooking.SalesEmployeeId;
        updatedBooking.CustomerId = existedBooking.CustomerId;
        _unitOfWork.Bookings.Update(updatedBooking);

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
                            throw new ArgumentNullException(null, "Can not find Voucher");
                        }
                    }
                    booking.Vouchers = voucherList;
                }

                if(salesEmployee == null)
                {
                    throw new ArgumentNullException(null, "Can not find Employee information");
                }

                if(customer == null)
                {
                    throw new ArgumentNullException(null, "Can not find Customer information");
                }

                if(servicePackage == null)
                {
                    throw new ArgumentNullException(null, "Can not find Service Package");
                }

                booking.SalesEmployee = salesEmployee;
                booking.Customer = customer;
                booking.ServicePackage = servicePackage;
                booking.BookingDate = DateTime.Now;

                await _unitOfWork.Bookings.AddAsync(booking);
                if (await _unitOfWork.SaveChangesAsync())
                {
                    return booking;
                }
                else
                {
                    // Log Cancellation
                }
                return null;
            }
            else
            {
                throw new ArgumentNullException(null, "Some Informations are null");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public IEnumerable<Booking>? GetAllBookings()
    {
        return _unitOfWork.Bookings.FindAll();
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

    public async Task<IEnumerable<Booking>?> GetAllBookingOfUser()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Users.FindAsync(userId);
        if(user == null)
        {
            throw new ArgumentNullException(null, "User Not Found");
        }
        return await _unitOfWork.Bookings.Get(b => b.SalesEmployeeId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Booking>?> GetAllBookingOfCustomer()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Customers.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, "Customer Not Found");
        }
        return await _unitOfWork.Bookings.Get(b => b.CustomerId == userId).AsNoTracking().ToListAsync();
    }
}
