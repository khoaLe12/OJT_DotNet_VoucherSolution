using Base.Core.Identity;
using Base.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IRepository;

public interface IRoleRepository : IBaseRepository<Role, Guid>
{
}
