using AutoMapper;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public IActionResult GetAllCustomer()
    {
        var result = _customerService.GetAllCustomers();
        if (result.IsNullOrEmpty() || result == null)
        {
            return NotFound("No Customer Found (empty) !!!");
        }
        return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
    }

    [HttpGet("All-Supported-Customers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseCustomerInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public async Task<IActionResult> GetAllSupportedCustomer()
    {
        try
        {
            var result = await _customerService.GetAllSupportedCustomer();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound("No Customer Found (empty) !!!");
            }
            return Ok(_mapper.Map<IEnumerable<ResponseCustomerInformationVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.ToString() + "\n Please Login First !!!!!");
        }
    }

    [HttpGet("{CustomerId}", Name = nameof(GetCustomerById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseCustomerInformationVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public async Task<IActionResult> GetCustomerById(Guid CustomerId)
    {
        if (ModelState.IsValid)
        {
            var result = await _customerService.GetCustomerById(CustomerId);
            if (result == null)
            {
                return NotFound("No Customer Found with the given id !!!");
            }
            return Ok(_mapper.Map<ResponseCustomerInformationVM>(result));
        }
        return BadRequest("Some properties are not valid !!!");
    }

    [Authorize(Policy = "Customers")]
    [HttpPost("Reset-Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    public async Task<IActionResult> ResetPassword(ResetPasswordVM resource)
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
            Message = "Some properties are not valid !!!"
        });
    }

    [Authorize(Policy = "Customers, SalesAdmin")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    public async Task<IActionResult> UpdateInformation([FromBody] UpdateInformationVM resource)
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
            Message = "Some properties are not valid !!!"
        });
    }

    [Authorize(Policy = "SalesAdmin")]
    [HttpPatch("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
    public async Task<IActionResult> PatchUpdate(Guid userId, [FromBody] JsonPatchDocument<Customer> patchDoc)
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
            Message = "Some properties are not valid !!!"
        });
    }
}
