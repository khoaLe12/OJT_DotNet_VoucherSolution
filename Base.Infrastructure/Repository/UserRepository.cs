using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace Base.Infrastructure.Repository;

internal class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
	public UserRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
	}
}
