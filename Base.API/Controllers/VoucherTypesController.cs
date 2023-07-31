using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "VoucherType")]
[Route("api/[controller]")]
[ApiController]
public class VoucherTypesController : ControllerBase
{
    private readonly IVoucherTypeService _voucherTypeService;
    private readonly IMapper _mapper;

    public VoucherTypesController(IVoucherTypeService voucherTypeService, IMapper mapper)
    {
        _voucherTypeService = voucherTypeService;
        _mapper = mapper;
    }

    [Authorize(Policy = "All")]
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherTypeVM>))]
    public IActionResult GetAllVoucherTypes()
    {
        var result = _voucherTypeService.GetAllVoucherTypes();
        return Ok(_mapper.Map<IEnumerable<ResponseVoucherTypeVM>>(result));
    }

    [Authorize(Policy = "All")]
    [HttpGet("all-delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherTypeVM>))]
    public IActionResult GetAllDeletedVoucherTypes()
    {
        var result = _voucherTypeService.GetAllDeletedVoucherTypes();
        return Ok(_mapper.Map<IEnumerable<ResponseVoucherTypeVM>>(result));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("{voucherTypeId}", Name = nameof(GetVoucherTypeById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseVoucherTypeVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public IActionResult GetVoucherTypeById(int voucherTypeId)
    {
        if (ModelState.IsValid)
        {
            var result = _voucherTypeService.GetVoucherTypeById(voucherTypeId);
            if(result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy",
                    Error = new List<string>() { "Can not find voucher type with the given id: " + voucherTypeId }
                });
            }
            return Ok(_mapper.Map<ResponseVoucherTypeVM>(result));
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ",
            Error = new List<string>() { "Invalid input" }
        });
    }

    [Authorize(Policy = "Write")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseVoucherTypeVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewVoucherType([FromBody] VoucherTypeVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherTypeService.AddNewVoucherType(_mapper.Map<VoucherType>(resource), resource.ServicePackageIds);
                if (result != null)
                {
                    return CreatedAtAction(nameof(GetVoucherTypeById),
                        new
                        {
                            voucherTypeId = result.Id
                        },
                        _mapper.Map<ResponseVoucherTypeVM>(result));
                }
                else
                {
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Đã có lỗi xảy ra"
                    });
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
        catch (ArgumentNullException ex)
        {
            var message = ex.Message;
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = message.Split(":").First(),
                Error = new List<string>() { "Can not find Service Package with the given id: " + message.Split(":").Last() }
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
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Tạo mới loại Voucher thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Update")]
    [HttpPut("{voucherTypeId}")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseVoucherTypeVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateVoucherType(int voucherTypeId, [FromBody] UpdatedVoucherTypeVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherTypeService.UpdateVoucherType(resource, voucherTypeId);
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
    [HttpPatch("{voucherTypeId}")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseVoucherTypeVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> PatchUpdate(int voucherTypeId, [FromBody] JsonPatchDocument<VoucherType> patchDoc)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherTypeService.PatchUpdate(voucherTypeId, patchDoc, ModelState);
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
    public async Task<IActionResult> SoftDeleteVoucherType(int id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherTypeService.SoftDelete(id);
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
