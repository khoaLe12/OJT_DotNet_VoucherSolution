using Base.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IRoleService
{
    IEnumerable<Role>? GetAllRole();
}
