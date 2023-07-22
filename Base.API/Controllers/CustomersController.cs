using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "Customer")]
[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomersController(ICustomerService customerService, IMapper mapper)
    {
        _customerService = customerService;
        _mapper = mapper;
    }

    [Authorize(Policy = "All")]
    [HttpGet("All-Customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    public IActionResult GetAllCustomer()
    {
        var result = _customerService.GetAllCustomers();
        return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("{CustomerId}", Name = nameof(GetCustomerById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseCustomerInformationVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetCustomerById(Guid CustomerId)
    {
        if (ModelState.IsValid)
        {
            var result = await _customerService.GetCustomerById(CustomerId);
            if (result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy"
                });
            }
            return Ok(_mapper.Map<ResponseCustomerInformationVM>(result));
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ"
        });
    }

    [Authorize(Policy = "Read")]
    [HttpGet("All-Supported-Customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllSupportedCustomer()
    {
        try
        {
            var result = await _customerService.GetAllSupportedCustomer();
            return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { "Can not find logged User with the given user id" }
            });
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
    }

    [HttpPost("Reset-Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> ResetPassword(ResetPasswordVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.ResetPassword(resource);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            return BadRequest(new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ"
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật mật khẩu thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateInformation([FromBody] UpdateInformationVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.UpdateInformation(resource);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            return BadRequest(new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ"
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

    [Authorize(Policy = "Write")]
    [HttpPatch("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> PatchUpdate(Guid userId, [FromBody] JsonPatchDocument<Customer> patchDoc)
    {
        try
        {
            if (ModelState.IsValid && patchDoc != null)
            {
                var result = await _customerService.PatchUpdate(userId, patchDoc, ModelState);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            return BadRequest(new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ"
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

    [Authorize(Policy = "Delete")]
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteUser(Guid userId)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.SoftDelete(userId);
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
