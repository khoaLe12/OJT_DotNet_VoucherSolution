using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Identity;

internal class CustomerManager<TUser> : UserManager<TUser> where TUser : Customer
{
    public CustomerManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators, IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger) 
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override async Task<IdentityResult> CreateAsync(TUser user, string password)
    {
        if (string.IsNullOrEmpty(user.UserName))
        {
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                user.UserName = user.PhoneNumber;
            }
            else if (!string.IsNullOrEmpty(user.Email))
            {
                user.UserName = user.Email;
            }
            else if (!string.IsNullOrEmpty(user.CitizenId))
            {
                user.UserName = user.CitizenId;
            }
        }

        return await base.CreateAsync(user, password);
    }
}
