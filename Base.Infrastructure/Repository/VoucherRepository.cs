using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Base.Infrastructure.Repository;

internal class VoucherRepository : BaseRepository<Voucher,int>, IVoucherRepository
{

	public VoucherRepository(ApplicationDbContext applicationDbContext, IServiceProvider serviceProvider) : base(applicationDbContext)
	{
	}
}
