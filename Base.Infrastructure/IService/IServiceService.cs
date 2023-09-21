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
    IEnumerable<Service> GetAllDeletedService();
    Task<Service?> GetServiceById(int id);
    Task<ServiceResponse> UpdateInformation(ServiceVM updatedService, int serviceId);
    Task<ServiceResponse> SoftDelete(int id);
    Task<ServiceResponse> RestoreService(int id);
}
