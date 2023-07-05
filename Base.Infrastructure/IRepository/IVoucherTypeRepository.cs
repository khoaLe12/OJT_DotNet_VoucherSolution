using Base.Core.Entity;
using Base.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IRepository;

public interface IVoucherTypeRepository : IBaseRepository<VoucherType, int>
{
}
