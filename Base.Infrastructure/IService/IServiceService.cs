using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IServiceService
{
    Task<Service?> AddNewService(Service service);
    IEnumerable<Service> GetAllService();
    Task<Service?> GetServiceById(int id);
    Task<ServiceResponse> UpdateInformation(Service updatedService, int serviceId);
    Task<ServiceResponse> SoftDelete(int id);
}
