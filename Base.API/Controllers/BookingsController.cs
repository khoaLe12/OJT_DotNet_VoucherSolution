using AutoMapper;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
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
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public IActionResult GetAllBookings()
        {
            var result = _bookingService.GetAllBookings();
            if(result.IsNullOrEmpty() || result == null)
            {
                return NotFound("No Booking Found (empty) !!!");
            }
            return Ok(_mapper.Map<IEnumerable<ResponseBookingVM>>(result));
        }

        [HttpGet("{bookingId}", Name = nameof(GetBookingById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBookingVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            if (ModelState.IsValid)
            {
                var result = await _bookingService.GetBookingById(bookingId);
                if(result == null)
                {
                    return NotFound("No Booking Found with the given id !!!");
                }
                return Ok(_mapper.Map<ResponseBookingVM>(result));
            }
            return BadRequest("Some properties are not valid !!!");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseBookingVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
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
                    return BadRequest("Some errors happened");
                }
                return BadRequest("Some properties are not valid !!!");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.ToString() + "\n\n Please Login First !!!!!");
            }
        }
    }
}
