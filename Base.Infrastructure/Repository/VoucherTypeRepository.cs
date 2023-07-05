using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class VoucherTypeRepository : BaseRepository<VoucherType, int>, IVoucherTypeRepository
{
	public VoucherTypeRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
	{
	}
}
