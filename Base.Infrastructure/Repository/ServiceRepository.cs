using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class ServiceRepository : BaseRepository<Service, int>, IServiceRepository
{
	public ServiceRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
	{
	}
}
