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

	public async Task<IEnumerable<User>?> GetUsersById(List<Guid> userIds)
	{
        var userList = new List<User>();
        foreach (Guid id in userIds)
        {
            var user = await FindAsync(id);
            if (user == null)
            {
                throw new ArgumentNullException(null, "Sales Employee Not Found with the given Id");
            }
            userList.Add(user);
        }
        return userList;
    }
}
