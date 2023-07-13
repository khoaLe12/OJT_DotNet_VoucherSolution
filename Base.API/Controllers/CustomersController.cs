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

    [HttpGet("All-Customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    public IActionResult GetAllCustomer()
    {
        var result = _customerService.GetAllCustomers();
        if (result.IsNullOrEmpty() || result == null)
        {
            return NotFound( new ServiceResponse
            {
                IsSuccess = true,
                Message = "empty"
            });
        }
        return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
    }

    [HttpGet("All-Supported-Customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllSupportedCustomer()
    {
        try
        {
            var result = await _customerService.GetAllSupportedCustomer();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound( new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Please Login First",
                Error = new List<string>() { ex.Message } 
            });
        }
    }

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
                return NotFound(new ServiceResponse { 
                    IsSuccess = false,
                    Message = "No Customer Found with the given id"
                });
            }
            return Ok(_mapper.Map<ResponseCustomerInformationVM>(result));
        }
        return BadRequest( new ServiceResponse 
        { 
            IsSuccess = false,
            Message = "Some properties are not valid" 
        });
    }

    [Authorize(Policy = "Customers")]
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
                Message = "Some properties are not valid"
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Reset Password Fail",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Customers, SalesAdmin")]
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
                Message = "Some properties are not valid"
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Update Information Fail",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "SalesAdmin")]
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
                Message = "Some properties are not valid"
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Update Account Fail",
                Error = new List<string>() { ex.Message }
            });
        }
    }
}
