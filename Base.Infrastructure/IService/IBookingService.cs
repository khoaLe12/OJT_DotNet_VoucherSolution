using Base.Core.Entity;
using Base.Core.ViewModel;
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
}
