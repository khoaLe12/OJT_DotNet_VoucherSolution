using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Entity;

public class ExpiredDateExtension
{
    [Key]
    public int Id { get; set; }

    //The Extension of which Voucher
    public Voucher? Voucher { get; set; }
    public int VoucherId { get; set; }


    //User who expand the voucher
    public User? SalesEmployee { get; set; }
    public Guid SalesEmployeeId { get; set; }


    [Column(TypeName = "money")]
    public Decimal Price { get; set; }


    public DateTime ExtendedDateTime { get; set; }
    public DateTime OldExpiredDate { get; set; }
    public DateTime NewExpiredDate { get; set; }
}
