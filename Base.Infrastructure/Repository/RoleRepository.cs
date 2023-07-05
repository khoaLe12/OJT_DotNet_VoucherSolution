using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class RoleRepository : BaseRepository<Role,int>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
	}

	public async Task<IEnumerable<Role>?> GetRolesByIds(List<int> roleIds)
	{
		try
		{
            var result = new List<Role>();

            foreach (int id in roleIds)
            {
                Role? r = await FindAsync(id);
                if (r != null)
                {
                    result.Add(r);
                }
            }
            return result;
        }
		catch (Exception ex)
		{
			//_logger.LogError(ex, "{Repo} GetRolesByIds method error", typeof(RoleRepository));
            return null; 
        }
	}
}
