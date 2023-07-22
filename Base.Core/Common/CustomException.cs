using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Common;

public class CustomException : Exception
{
    public override string Message { get; }
    public IEnumerable<string>? Errors { get; set; }

    public CustomException(string message)
	{
		Message = message;
    }
}
