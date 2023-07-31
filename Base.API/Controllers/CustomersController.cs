using AutoMapper;
using Base.API.Services;
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
    private readonly IMailService _mailService;

    public CustomersController(ICustomerService customerService, IMapper mapper, IMailService mailService)
    {
        _customerService = customerService;
        _mapper = mapper;
        _mailService = mailService;
    }

    [Authorize(Policy = "All")]
    [HttpGet("all-customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    public IActionResult GetAllCustomer()
    {
        var result = _customerService.GetAllCustomers();
        return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
    }

    [Authorize(Policy = "All")]
    [HttpGet("all-deleted-customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    public IActionResult GetAllDeletedCustomer()
    {
        var result = _customerService.GetAllDeletedCustomers();
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

    [AllowAnonymous]
    [HttpPost("forget-password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    public async Task<IActionResult> ForgetPassword([FromQuery] string email)
    {
        if (ModelState.IsValid)
        {
            var result = await _customerService.ForgetPasswordAsync(email);
            if (result.IsSuccess)
            {
                if (result.ConfirmEmailUrl is not null)
                {
                    var url = result.ConfirmEmailUrl;
                    await _mailService.SendMailAsync(new Message
                    {
                        To = result.LoginCustomer!.Email,
                        Subject = "Reset Password",
                        Content = "<h2>Follow the instructions to reset your password</h2>" +
                            $"<p>To reset your password <a href='{url}'>Click here</a></p>"
                    });
                    result.LoginCustomer = null;
                    result.ConfirmEmailUrl = null;
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        else
        {
            return BadRequest(new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Errors = new List<string>() { "Invalid input" }
            });
        }
    }

    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateInformation([FromForm] UpdateInformationVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.UpdateInformation(resource);
                if (result.IsSuccess)
                {
                    if (result.ConfirmEmailUrl is not null)
                    {
                        var url = result.ConfirmEmailUrl;
                        await _mailService.SendMailAsync(new Message
                        {
                            To = result.LoginCustomer!.Email,
                            Subject = "Confirm your email",
                            Content = "<h2>Welcome to Voucher Solution</h2>" +
                                    $"<p>Please confirm your email by <a href='{url}'>clicking here</a> </p>"
                        });
                    }
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
    [HttpPatch("{CustomerId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> PatchUpdate(Guid CustomerId, [FromBody] JsonPatchDocument<Customer> patchDoc)
    {
        try
        {
            if (ModelState.IsValid && patchDoc != null)
            {
                var result = await _customerService.PatchUpdate(CustomerId, patchDoc, ModelState);
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

    [Authorize(Policy = "Update")]
    [HttpPatch("assign-supporters/{CustomerId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AssignSupporters(Guid CustomerId, [FromBody] IEnumerable<AssignSupporterVM> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.AssignSupporter(CustomerId, resource);
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
                Message = "Dữ liệu không hợp lệ",
                Errors = new List<string>() { "Invalid Input" }
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
    [HttpDelete("{CustomerId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteUser(Guid CustomerId)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.SoftDelete(CustomerId);
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
