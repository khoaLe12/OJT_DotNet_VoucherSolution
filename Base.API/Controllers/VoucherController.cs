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

[Authorize(Policy = "Voucher")]
[Route("api/[controller]")]
[ApiController]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    private readonly IMapper _mapper;

    public VoucherController(IVoucherService voucherService, IMapper mapper)
    {
        _voucherService = voucherService;
        _mapper = mapper;
    }

    [Authorize(Policy = "All")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherVM>))]
    public IActionResult GetAllVoucher()
    {
        var result = _voucherService.GetAllVoucher();
        return Ok(_mapper.Map<IEnumerable<ResponseVoucherVM>>(result));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("{VoucherId}", Name = nameof(GetVoucherById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseVoucherVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetVoucherById(int VoucherId)
    {
        if (ModelState.IsValid)
        {
            var result = await _voucherService.GetVoucherById(VoucherId);
            if (result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy Voucher",
                    Error = new List<string>() { "Can not find voucher with the given id: " + VoucherId }
                });
            }
            return Ok(_mapper.Map<ResponseVoucherVM>(result));
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ"
        });
    }

    [Authorize(Policy = "Read")]
    [HttpGet("User")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllVoucherOfUser()
    {
        try
        {
            var result = await _voucherService.GetAllVoucherOfUser();
            return Ok(_mapper.Map<IEnumerable<ResponseVoucherVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Hành động không hợp lệ",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Customer")]
    [HttpGet("Customer")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllVoucherOfCustomer()
    {
        try
        {
            var result = await _voucherService.GetAllVoucherOfCustomer();
            return Ok(_mapper.Map<IEnumerable<ResponseVoucherVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { ex.Message }
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { ex.Message }
            });
        }
    }
    
    [Authorize(Policy = "Write")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseVoucherVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewVoucher([FromBody] VoucherVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherService.AddNewVoucher(_mapper.Map<Voucher>(resource), resource.CustomerId, resource.VoucherTypeId);
                if (result != null)
                {
                    return CreatedAtAction(nameof(GetVoucherById),
                        new
                        {
                            VoucherId = result.Id
                        },
                        _mapper.Map<ResponseVoucherVM>(result));
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
        catch (ArgumentNullException ex)
        {
            var message = ex.Message;
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = message.Split(":").First(),
                Error = new List<string>() { "Can not find with the given id: " + message.Split(":").Last() }
            });
        }
        catch (CustomException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = ex.Errors
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Tạo mới voucher thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Tạo mới Voucher thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Update")]
    [HttpPut("{voucherId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateVoucher(int voucherId, [FromBody] UpdatedVoucherVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherService.UpdateVoucher(resource, voucherId);
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
                    Message = "Dữ liệu không hợp lệ"
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật voucher thất bại",
                Error = new List<string>() { ex.Message }
            });
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
    [HttpPatch("{voucherId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> PatchUpdate(int voucherId, [FromBody] JsonPatchDocument<Voucher> patchDoc)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherService.PatchUpdate(voucherId, patchDoc, ModelState);
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
                    Message = "Dữ liệu không hợp lệ"
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
    public async Task<IActionResult> SoftDeleteVoucher(int id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherService.SoftDelete(id);
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
