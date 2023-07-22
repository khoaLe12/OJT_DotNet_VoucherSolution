using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "Booking")]
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

    [Authorize(Policy = "All")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseBookingVM>))]
    public IActionResult GetAllBookings()
    {
        var result = _bookingService.GetAllBookings();
        return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
    }

    [Authorize(Policy = "Read")]
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
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy",
                    Error = new List<string>() { "Can not find Booking with the given id: " + bookingId }
                });
            }
            return Ok(_mapper.Map<ResponseBookingVM>(result));
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ",
            Error = new List<string>() { "Invalid input" }
        });
    }

    [Authorize(Policy = "Read")]
    [HttpGet("User")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseBookingVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllBookingsOfUser()
    {
        try
        {
            var result = await _bookingService.GetAllBookingOfUser();
            return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
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
                Message = "Không tìm thấy người dùng",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Customer")]
    [HttpGet("Customer")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseBookingVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllBookingsOfCustomer()
    {
        try
        {
            var result = await _bookingService.GetAllBookingOfCustomer();
            return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
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
        catch(ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy người dùng",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Write")]
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
                        Message = "Đã có lỗi xảy ra"
                    });
                }
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Error = new List<string>() { "Invalid input" }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Tạo mới Booking thất bại",
                Error = new List<string>() { ex.Message }
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
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Tạo mới Booking thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpPost("Vouchers/{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> ApplyVouchers(int bookingId, [FromBody] IEnumerable<int> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _bookingService.ApplyVouchers(bookingId, resource);
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

    [Authorize(Policy = "Update")]
    [HttpPut("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateBooking(int bookingId, [FromBody] UpdatedBookingVM resource)
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
    [HttpDelete("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteBooking(int bookingId)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _bookingService.SoftDelete(bookingId);
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
