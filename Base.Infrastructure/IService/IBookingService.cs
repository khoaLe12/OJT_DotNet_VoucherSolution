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
    Task<Booking?> AddNewBooking(Booking? booking, Guid? CustomerId, int? ServicePackageId, IEnumerable<int>? VoucherIds);
    IEnumerable<Booking>? GetAllBookings();
    Task<Booking?> GetBookingById(int id);
    Task<ServiceResponse> PatchUpdate(int bookingId, JsonPatchDocument<Booking> patchDoc, ModelStateDictionary ModelState);
    Task<ServiceResponse> UpdateBooking(Booking? updatedBooking, int bookingId);
    Task<IEnumerable<Booking>?> GetAllBookingOfUser();
    Task<IEnumerable<Booking>?> GetAllBookingOfCustomer();
}
