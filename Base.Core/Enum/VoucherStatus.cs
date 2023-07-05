using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Enum;

public enum VoucherStatus
{
    Expired = 1,
    Usable = 2,
    OutOfValue = 3,
    Blocked = 4,
}
