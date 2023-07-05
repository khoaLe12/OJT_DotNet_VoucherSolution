using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class BookingRepository : BaseRepository<Booking,int> ,IBookingRepository
{
	public BookingRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
	{
	}
}
