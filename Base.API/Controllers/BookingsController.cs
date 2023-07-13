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
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IMapper _mapper;

    public BookingsController(IBookingService bookingService, IMapper mapper)
    {
        _bookingService = bookingService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseBookingVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    public IActionResult GetAllBookings()
    {
        var result = _bookingService.GetAllBookings();
        if(result.IsNullOrEmpty() || result == null)
        {
            return NotFound( new ServiceResponse
            {
                IsSuccess = true,
                Message = "empty"
            });
        }
        return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
    }

    [HttpGet("User")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseBookingVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllBookingsOfUser()
    {
        try
        {
            var result = await _bookingService.GetAllBookingOfUser();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
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

    [HttpGet("Customer")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseBookingVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllBookingsOfCustomer()
    {
        try
        {
            var result = await _bookingService.GetAllBookingOfCustomer();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
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
        catch(ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpGet("{bookingId}", Name = nameof(GetBookingById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBookingVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        if (ModelState.IsValid)
        {
            var result = await _bookingService.GetBookingById(bookingId);
            if (result == null)
            {
                return NotFound(new ServiceResponse {
                    IsSuccess = false,
                    Message = "No Booking Found with the given id"
                });
            }
            return Ok(_mapper.Map<ResponseBookingVM>(result));
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Some properties are not valid"
        });
    }

    [Authorize(Policy = "SalesEmployee")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseBookingVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewBooking([FromBody] BookingVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _bookingService.AddNewBooking(_mapper.Map<Booking>(resource), resource.CustomerId, resource.ServicePackageId, resource.VoucherIds);
                if (result != null)
                {
                    return CreatedAtAction(nameof(GetBookingById),
                        new
                        {
                            bookingId = result.Id
                        },
                        _mapper.Map<ResponseBookingVM>(result));
                }
                else
                {
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Some errors happened"
                    });
                }
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some properties are not valid"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Create new Booking Fail",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Create new Booking Fail",
                Error = new List<string>() { ex.Message }
            });
        }
        catch(DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Create new Booking Fail",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpPut("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateInformation(int bookingId, [FromBody]BookingVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _bookingService.UpdateBooking(_mapper.Map<Booking>(resource), bookingId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            return BadRequest(new ServiceResponse
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
                Message = "Update Booking Fail",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Update Booking Fail",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpPatch("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> PatchUpdate(int bookingId, [FromBody] JsonPatchDocument<Booking> patchDoc)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _bookingService.PatchUpdate(bookingId, patchDoc, ModelState);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            return BadRequest(new ServiceResponse
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
                Message = "Update Booking Fail",
                Error = new List<string>() { ex.Message }
            });
        }
    }
}
