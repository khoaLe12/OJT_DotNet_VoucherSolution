using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Entity;

public class Role : IdentityRole<int>
{
    public IEnumerable<User>? Users { get; set; }
}
