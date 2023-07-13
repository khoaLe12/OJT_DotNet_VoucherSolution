using Base.Core.Identity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class RoleRepository : BaseRepository<Role, Guid>, IRoleRepository
{
	public RoleRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
	{
	}
}
