using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Enum;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
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

    public async Task<ServiceResponse> ApplyVouchers(int bookingId, IEnumerable<int> voucherIds)
    {
        var userId = _currentUserService.UserId;

        Expression<Func<Booking, object>>[] includes = {
            b => b.ServicePackage!,
            b => b.ServicePackage!.ValuableVoucherTypes!,
            b => b.Vouchers!
        };

        var existedBooking = await _unitOfWork.Bookings.FindAsync(bookingId);
        if (existedBooking == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy booking",
                Error = new List<string>() { "Can not find booking with the given id: " + bookingId }
            };
        }

        if(existedBooking.BookingStatus == (int)BookingStatus.Confirmed)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không thể áp dụng voucher",
                Error = new List<string>() { "Payment has been completed, voucher can not be applied" }
            };
        }

        if(existedBooking.BookingStatus == (int)BookingStatus.Cancelled)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không thể áp dụng voucher",
                Error = new List<string>() { "Booking has been cancelled, voucher can not be applied" }
            };
        }

        var appliableVoucherType = existedBooking.ServicePackage?.ValuableVoucherTypes?.Where(vt => vt.IsAvailable);
        if(appliableVoucherType == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = $"Giảm giá hiện không được áp dụng cho gói dịch vụ {existedBooking.ServicePackage!.ServicePackageName}",
                Error = new List<string>() { "This booked service package have no promotion" }
            };
        }

        var vouchers = new List<Voucher>();
        foreach(var id in voucherIds)
        {
            var voucher = _unitOfWork.Vouchers.Get(v => v.Id == id, v => v.VoucherType!).FirstOrDefault();
            if (voucher == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy voucher",
                    Error = new List<string>() { $"Can not find voucher with id '{id}'" }
                };
            }

            if(voucher.VoucherStatus != 2)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"Voucher '{voucher.VoucherType!.TypeName}' không còn giá trị sử dụng",
                    Error = new List<string>() { $"Status of Voucher is '{Enum.GetName(typeof(VoucherStatus), voucher.VoucherStatus)}'" }
                };
            }

            if(existedBooking.CustomerId != voucher.CustomerId)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"Không thể sử dụng voucher '{voucher.VoucherType!.TypeName}'",
                    Error = new List<string>() { $"Voucher '{voucher.Id}' does not belong to customer '{existedBooking.CustomerId}'" }
                };
            }

            if (!appliableVoucherType.Contains(voucher.VoucherType))
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"Voucher '{voucher.VoucherType!.TypeName}' không được áp dụng cho gói dịch vụ này",
                    Error = new List<string>() { $"Voucher '{voucher.Id}' can not be applied for the booking '{existedBooking.Id}'" }
                };
            }

            vouchers.Add(voucher);
        }

        if (vouchers.IsNullOrEmpty())
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy voucher",
                Error = new List<string>() { "Voucher list is null" }
            };
        }

        if(existedBooking.Vouchers == null)
        {
            existedBooking.Vouchers = vouchers;
        }
        else
        {
            existedBooking.Vouchers = existedBooking.Vouchers.Concat(vouchers).ToList();
        }

        if(await _unitOfWork.SaveChangesAsync())
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
                Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
            };
        }
    }

    public async Task<ServiceResponse> UpdateBooking(Booking updatedBooking, int bookingId)
    {
        var existedBooking = await _unitOfWork.Bookings.Get(b => b.Id == bookingId).AsNoTracking().FirstOrDefaultAsync();
        if(existedBooking == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Gói dịch vụ chưa được đặt",
                Error = new List<string>() { "Can not find booking with the given id: " + bookingId }
            };
        }

        if (!Enum.IsDefined(typeof(BookingStatus), updatedBooking.BookingStatus))
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Trạng thái của Booking không hợp lệ",
                Error = new List<string>() { $"The booking status '{updatedBooking.BookingStatus}' does not exist" }
            };
        }

        updatedBooking.Id = bookingId;
        updatedBooking.SalesEmployeeId = existedBooking.SalesEmployeeId;
        updatedBooking.CustomerId = existedBooking.CustomerId;
        updatedBooking.ServicePackageId = existedBooking.ServicePackageId;
        updatedBooking.IsDeleted = existedBooking.IsDeleted;
        _unitOfWork.Bookings.Update(updatedBooking);

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

    public async Task<Booking?> AddNewBooking(Booking booking, Guid CustomerId, int ServicePackageId, IEnumerable<int>? VoucherIds)
    {
        var salesEmployee = await _unitOfWork.Users.Get(u => u.Id == _currentUserService.UserId, u => u.Customers!).FirstOrDefaultAsync();
        var customer = await _unitOfWork.Customers.FindAsync(CustomerId);
        var servicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == ServicePackageId, sp => sp.ValuableVoucherTypes!).FirstOrDefaultAsync();

        if (!Enum.IsDefined(typeof(BookingStatus), booking.BookingStatus))
        {
            throw new CustomException("Trạng thái của Booking không hợp lệ")
            {
                Errors = new List<string>() { $"The booking status '{booking.BookingStatus}' does not exist" }
            };
        }

        if (salesEmployee == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin người dùng{_currentUserService.UserId}");
        }

        if (customer == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin khách hàng:{CustomerId}");
        }

        if (salesEmployee.Customers.IsNullOrEmpty() || !salesEmployee.Customers!.Contains(customer))
        {
            throw new CustomException($"Bạn không hỗ trợ khách hàng '{customer.Name}'")
            {
                Errors = new List<string>() { $"User '{salesEmployee.Name}' does not support Customer '{customer.Name}'" }
            };
        }

        if (servicePackage == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy gói dịch vụ:{ServicePackageId}");
        }

        if (VoucherIds != null)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            var voucherList = new ConcurrentBag<Voucher>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.1 * 2))
            };
            Parallel.ForEach(VoucherIds, options, (id, state) =>
            {
                var voucher = _unitOfWork.Vouchers.Get(v => v.Id == id, v => v.VoucherType!).FirstOrDefault();
                if (voucher == null)
                {
                    exceptions.Enqueue(new ArgumentNullException(null, $"Không tìm thấy voucher:{id}"));
                    state.Stop();
                    return;
                }

                if (voucher.CustomerId != customer.Id)
                {
                    exceptions.Enqueue(new CustomException($"Khách hàng '{customer.Name}' không sở hữu voucher '{voucher.Id}'")
                    {
                        Errors = new List<string>() { $"Voucher '{voucher.Id}' does not belong to '{customer.Name}'" }
                    });
                    state.Stop();
                    return;
                }

            if (!servicePackage.ValuableVoucherTypes!.Contains(voucher.VoucherType))
            {
                exceptions.Enqueue(new CustomException($"Voucher '{voucher.VoucherType!.TypeName}' không áp dụng cho gói dịch vụ '{servicePackage.ServicePackageName}'")
                {
                    Errors = new List<string>() { $"Voucher type '{voucher.VoucherType!.TypeName}' are not applied on '{servicePackage.ServicePackageName}'" }
                });
                    state.Stop();
                    return;
                }

                voucherList.Add(voucher);
            });

            if (!exceptions.IsNullOrEmpty())
            {
                throw exceptions.First();
            }

            booking.Vouchers = voucherList.ToList();
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
        return null;
    }

    public IEnumerable<Booking> GetAllBookings()
    {
        return _unitOfWork.Bookings.FindAll().Where(b => !b.IsDeleted);
    }

    public async Task<Booking?> GetBookingById(int id)
    {
        Expression<Func<Booking, bool>> where = b => !b.IsDeleted && b.Id == id;
        Expression<Func<Booking, object>>[] includes = {
            b => b.Customer!,
            b => b.SalesEmployee!
        };
        return await _unitOfWork.Bookings.Get(where, includes)
            .Include(nameof(Booking.ServicePackage) + "." + nameof(ServicePackage.Services))
            .Include(nameof(Booking.Vouchers) + "." + nameof(Voucher.VoucherType))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Booking>> GetAllBookingOfUser()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Users.FindAsync(userId);
        if(user == null)
        {
            throw new ArgumentNullException(null, $"User Not Found with the given id: {userId}");
        }
        return await _unitOfWork.Bookings.Get(b => b.SalesEmployeeId == userId && !b.IsDeleted).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetAllBookingOfCustomer()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Customers.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, $"Customer Not Found with the given id: {userId}");
        }
        return await _unitOfWork.Bookings.Get(b => b.CustomerId == userId && !b.IsDeleted ).AsNoTracking().ToListAsync();
    }

    public async Task<ServiceResponse> SoftDelete(int bookingId)
    {
        var existedBooking = await _unitOfWork.Bookings.Get(b => b.Id == bookingId && !b.IsDeleted).FirstOrDefaultAsync();
        if(existedBooking == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy booking",
                Error = new List<string>() { "Can not find booking with the given id: " + bookingId }
            };
        }

        existedBooking.IsDeleted = true;

        var log = new Log
        {
            Type = (int)AuditType.Delete,
            TableName = nameof(Booking),
            PrimaryKey = bookingId.ToString()
        };

        await _unitOfWork.AuditLogs.AddAsync(log);

        if(await _unitOfWork.SaveChangesAsync())
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
