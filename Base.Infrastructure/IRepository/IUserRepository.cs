using Base.Core.Entity;
using Base.Infrastructure.Data;

namespace Base.Infrastructure.IRepository;

public interface IUserRepository : IBaseRepository<User, Guid>
{
    Task<IEnumerable<User>?> GetUsersById(List<Guid> userIds);
}
