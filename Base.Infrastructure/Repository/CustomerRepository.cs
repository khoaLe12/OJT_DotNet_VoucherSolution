using Base.Core.Identity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repository;

internal class CustomerRepository : BaseRepository<Customer,Guid>, ICustomerRepository
{
	public CustomerRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
	}


}
