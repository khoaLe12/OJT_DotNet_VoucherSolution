using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IBookingService
{
    Task<ServiceResponse> ApplyVouchers(int bookingId, IEnumerable<int> voucherIds);
    Task<Booking?> AddNewBooking(Booking booking, Guid CustomerId, int ServicePackageId, IEnumerable<int>? VoucherIds);
    IEnumerable<Booking> GetAllBookings();
    IEnumerable<Booking> GetAllDeletedBookings();
    Task<Booking?> GetBookingById(int id);
    Task<ServiceResponse> UpdateBooking(UpdatedBookingVM updatedBooking, int bookingId);
    Task<IEnumerable<Booking>> GetAllBookingOfUserById(Guid userId);
    Task<IEnumerable<Booking>> GetAllBookingOfUser();
    Task<IEnumerable<Booking>> GetAllBookingOfCustomerById(Guid customerId);
    Task<IEnumerable<Booking>> GetAllBookingOfCustomer();
    Task<ServiceResponse> SoftDelete(int bookingId);
    Task<ServiceResponse> RestoreBooking(int id);
}
