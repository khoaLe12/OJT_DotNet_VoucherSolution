using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class ExpiredDateExtensionRepository : BaseRepository<ExpiredDateExtension,int>, IExpiredDateExtensionRepository
{
	public ExpiredDateExtensionRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
	{
	}
}
