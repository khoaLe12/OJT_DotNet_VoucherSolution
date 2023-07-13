using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IMapper _mapper;

        public ServicesController(IServiceService serviceService, IMapper mapper)
        {
            _serviceService = serviceService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServiceVM>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        public IActionResult GetAllServices()
        {
            var result = _serviceService.GetAllService();
            if(result.IsNullOrEmpty() || result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<Service>, IEnumerable<ResponseServiceVM>>(result));
        }


        [HttpGet("{serviceId}", Name = nameof(GetServiceById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseServiceVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetServiceById(int serviceId)
        {
            if (ModelState.IsValid)
            {
                var result = await _serviceService.GetServiceById(serviceId);
                if (result == null)
                {
                    return NotFound( new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "No Service Found with the given id"
                    });
                }
                return Ok(_mapper.Map<Service, ResponseServiceVM>(result));
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some properties are not valid"
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseServiceVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> AddNewService([FromBody] ServiceVM resource)
        {
            if (ModelState.IsValid)
            {
                var newService = _mapper.Map<ServiceVM,Service>(resource);
                var result = await _serviceService.AddNewService(newService);
                if (result != null)
                {
                    return CreatedAtAction(nameof(GetServiceById),
                        new
                        {
                            serviceId = result.Id
                        },
                        _mapper.Map<Service, ResponseServiceVM>(result));
                }
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some errors happened"
                });
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some properties are not valid"
            });
        }
    }
}
