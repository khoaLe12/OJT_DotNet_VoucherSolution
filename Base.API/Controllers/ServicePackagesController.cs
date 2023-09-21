using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "ServicePackage")]
[Route("api/[controller]")]
[ApiController]
public class ServicePackagesController : ControllerBase
{
    private readonly IServicePackageService _servicePackageService;
    private readonly IMapper _mapper;

    public ServicePackagesController(IServicePackageService servicePackageService, IMapper mapper)
    {
        _servicePackageService = servicePackageService;
        _mapper = mapper;
    }

    [Authorize(Policy = "All")]
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServicePackageVM>))]
    public IActionResult GetAllServicePackages()
    {
        var result = _servicePackageService.GetALlServicePackage();
        return Ok(_mapper.Map<IEnumerable<ServicePackage>, IEnumerable<ResponseServicePackageVM>>(result));
    }

    [Authorize(Policy = "All")]
    [HttpGet("all-delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServicePackageVM>))]
    public IActionResult GetAllDeletedServicePackages()
    {
        var result = _servicePackageService.GetAllDeletedServicePackage();
        return Ok(_mapper.Map<IEnumerable<ServicePackage>, IEnumerable<ResponseServicePackageVM>>(result));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("{servicePackageId}", Name = nameof(GetServicePackageById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseServicePackageVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public IActionResult GetServicePackageById(int servicePackageId)
    {
        if (ModelState.IsValid)
        {
            var result = _servicePackageService.GetServicePackageById(servicePackageId);
            if (result == null)
            {
                return NotFound( new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy",
                    Error = new List<string>() { "Can not find service package with the given id: " + servicePackageId }
                });
            }
            return Ok(_mapper.Map<ServicePackage, ResponseServicePackageVM>(result));
        }
        return BadRequest( new ServiceResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ",
            Error = new List<string>() { "Invalid input" }
        });
    }

    [Authorize(Policy = "Write")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseServicePackageVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewServicePackage([FromBody] ServicePackageVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var newServicePackage = _mapper.Map<ServicePackage>(resource);
                var result = await _servicePackageService.AddNewServicePackage(newServicePackage, resource.ServicesIds);
                if (result != null)
                {
                    return CreatedAtAction(nameof(GetServicePackageById),
                        new
                        {
                            servicePackageId = result.Id,
                        },
                        _mapper.Map<ResponseServicePackageVM>(result));
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
                Message = "Dữ liệu không hợp lệ",
                Error = new List<string>() { "Invalid input" }
            });
        }
        catch (ArgumentNullException ex)
        {
            var messages = ex.Message.Split(":");
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = messages.First(),
                Error = new List<string>() { "Can not find with the given id: " + messages.Last() }
            });
        }
        catch (CustomException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = ex.Errors,
                IsRestored = ex.IsRestored
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Tạo mới gói dịch vụ thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Update")]
    [HttpPatch("VoucherTypes/{servicePackageId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateVoucherTypesOnServicePackage(int servicePackageId, IEnumerable<int> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _servicePackageService.UpdateVoucherTypesOnServicePackage(servicePackageId, resource);
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
    }

    [Authorize(Policy = "Update")]
    [HttpPut("{servicePackageId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateServicePackage(int servicePackageId, [FromBody] UpdatedServicePackageVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _servicePackageService.UpdateInformation(servicePackageId, resource);
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
    }

    [Authorize(Policy = "Update")]
    [HttpPatch("Services/{servicePackageId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateServicesInServicePackage(int servicePackageId, [FromBody] IEnumerable<int> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _servicePackageService.UpdateServiceOfServicePackage(resource, servicePackageId);
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
    }

    [Authorize(Policy = "Delete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteServicePackage(int id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _servicePackageService.SoftDelete(id);
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

    [Authorize(Policy = "Delete")]
    [HttpDelete("batch")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteServicePackages([FromBody] IEnumerable<int> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _servicePackageService.SoftDeleteBatch(resource);
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
    }

    [Authorize(Policy = "Restore")]
    [HttpPatch("restore-servicepackage/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> RestoreServicePackage(int id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _servicePackageService.RestoreServicePackage(id);
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
                Message = "Khôi phục thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }
}