using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "Service")]
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

    [Authorize(Policy = "All")]
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServiceVM>))]
    public IActionResult GetAllServices()
    {
        var result = _serviceService.GetAllService();
        return Ok(_mapper.Map<IEnumerable<Service>, IEnumerable<ResponseServiceVM>>(result));
    }

    [Authorize(Policy = "All")]
    [HttpGet("all-delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServiceVM>))]
    public IActionResult GetAllDeletedServices()
    {
        var result = _serviceService.GetAllDeletedService();
        return Ok(_mapper.Map<IEnumerable<Service>, IEnumerable<ResponseServiceVM>>(result));
    }

    [Authorize(Policy = "Read")]
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
                    Message = "Không tìm thấy dịch vụ",
                    Error = new List<string>() { "Can not find service with the given id: " + serviceId }
                });
            }
            return Ok(_mapper.Map<Service, ResponseServiceVM>(result));
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ"
        });
    }

    [Authorize(Policy = "Write")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseServiceVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewService([FromBody] ServiceVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var newService = _mapper.Map<ServiceVM, Service>(resource);
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
                    Message = "Đã có lỗi xảy ra"
                });
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message
            });
        }
    }

    [Authorize(Policy = "Update")]
    [HttpPut("{serviceId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateService(int serviceId, [FromBody] ServiceVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _serviceService.UpdateInformation(resource, serviceId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Thông tin cập nhật không hợp lệ"
                });
            }
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Delete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteService(int id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _serviceService.SoftDelete(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ",
                    Error = new List<string>() { "Invalid input" }
                });
            }
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }
}
