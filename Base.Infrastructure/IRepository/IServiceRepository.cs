using Base.Core.Entity;
using Base.Infrastructure.Data;

namespace Base.Infrastructure.IRepository;

public interface IServiceRepository : IBaseRepository<Service, int>
{
}
