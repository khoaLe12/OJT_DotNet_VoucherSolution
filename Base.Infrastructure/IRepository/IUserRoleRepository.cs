/*using Base.Core.Entity;
using Base.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IRepository
{
    public interface IUserRoleRepository : IBaseRepository<UserRole, Guid>
    {
        Task<IEnumerable<Role>?> GetAllRolesOfUserById(Guid userId);
        Task<bool> AddRangeAsync(User user, List<int> roleIds);
    }
}*/
