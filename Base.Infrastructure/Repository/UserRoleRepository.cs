/*using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Base.Infrastructure.Repository;

internal class UserRoleRepository : BaseRepository<UserRole, Guid>, IUserRoleRepository
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IRoleRepository _roleRepository;
    public UserRoleRepository(IServiceScopeFactory serviceScopeFactory ,IRoleRepository roleRepository, ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<Role>?> GetAllRolesOfUserById(Guid userId)
    {
        try
        {
            List<Role> roles = new List<Role>();
            var userRoles = Get(ur => ur.UserId == userId,
                new Expression<Func<UserRole, object>>[]
                {
                ur => ur.Role
                });
            await userRoles.ForEachAsync(ur => roles.Add(ur.Role));
            return roles;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "{Repo} GetAllRolesOfUserById method error", typeof(UserRoleRepository));
            return null;
        }
    }

    public async Task<bool> AddRangeAsync(User user, List<int> roleIds)
    {
        try
        {
            bool check = true;
            var userRoleList = new List<UserRole>();
            foreach (int id in roleIds)
            {
                var role = await _roleRepository.FindAsync(id);
                if (role != null)
                {
                    UserRole ur = new UserRole
                    {
                        User = user,
                        Role = role
                    };
                    userRoleList.Add(ur);
                }
                else
                {
                    check = false;
                }
            }

            if (check)
            {
                await AddRangeAsync(userRoleList.ToList());
            }
            return check;
        }
        catch(Exception ex)
        {
            //_logger.LogError(ex, "{Repo} AddRangeAsync(User,List<int>) method error", typeof(UserRoleRepository));
            return false;
        }
    }
}*/



#region IServiceScopeFactory
//Parallel Programming

//Start multi threads, need to use 2 different DbContext
//One context is used for finding Role by Id, and make a list of UserRole
//Other context is used for adding multi UserRole
//And use that scope context to save all changes

// IServiceScopeFactory create many IServiceScope
// A IServiceScope provide an IServiceProvider (Each scope has only one provider)
// A IServiceProvider create many instances of different registry services (resolves services)

//private readonly IServiceScopeFactory _serviceScopeFactory;
//_serviceScopeFactory = serviceScopeFactory;

/*
Create scope using IServiceScopeFactory
using (var scope = _serviceScopeFactory.CreateScope())
{}

Create dbContext service using provider of the scope
var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
*/

/*roleIds.AsParallel()
            .WithDegreeOfParallelism(Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.2) * 2)))
            .ForAll(id =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var role = dbContext.Roles.AsNoTracking().FirstOrDefault(r => r.Id == id);
                    if (role != null)
                    {
                        UserRole ur = new UserRole
                        {
                            UserId = userId,
                            RoleId = id
                        };
                        userRoleList.Add(ur);
                    }
                    else
                    {
                        check = false;
                    }
                } 
            });*/


/*var userRoleList = new ConcurrentBag<UserRole>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.2) * 2))
            };

            Parallel.ForEach(roleIds, options, (id, state) =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var role = dbContext.Roles.AsNoTracking().FirstOrDefault(r => r.Id == id);
                    if (role != null)
                    {
                        UserRole ur = new UserRole
                        {
                            User = user,
                            Role = role,
                        };
                        userRoleList.Add(ur);
                        // The role is found by another DbContext
                        // So we need to change its state on the scoped DbContext to Unchanged
                        _applicationDbContext.Roles.Entry(role).State = EntityState.Unchanged;
                    }
                    else
                    {
                        check = false;
                        state.Break();
                    }
                }
            });*/

#endregion

#region async delegate "Action" = async void (should not use async void)
/*roleIds.ForEach(async id =>
        {
            var role = await _roleRepository.FindAsync(id);
            if (role != null)
            {
                UserRole ur = new UserRole
                {
                    UserId = userId,
                    RoleId = id
                };
                userRoleList.Add(ur);
            }
            else
            {
                check = false;
            }
        });*/
#endregion