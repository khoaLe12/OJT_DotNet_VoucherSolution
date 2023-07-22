using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;

namespace Base.Infrastructure.Repository;

internal class AuditRepository : BaseRepository<Log,int>, IAuditRepository
{
	public AuditRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
	{ 
	}
}
