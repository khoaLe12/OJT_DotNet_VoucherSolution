using Base.Core.Entity;
using Base.Infrastructure.Data;

namespace Base.Infrastructure.IRepository;

public interface IExpiredDateExtensionRepository : IBaseRepository<ExpiredDateExtension, int>
{
}
