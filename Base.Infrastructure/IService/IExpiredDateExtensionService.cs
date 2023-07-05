using Base.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IExpiredDateExtensionService
{
    Task<ExpiredDateExtension?> AddNewExpiredDateExtension(ExpiredDateExtension? expiredDateExtension, int? VoucherId);
    IEnumerable<ExpiredDateExtension>? GetAllExpiredDateExtensions();
    Task<ExpiredDateExtension?> GetExpiredDateExtensionById(int id);
}
