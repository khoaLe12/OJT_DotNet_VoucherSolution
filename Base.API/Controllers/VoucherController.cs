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
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly IMapper _mapper;

        public VoucherController(IVoucherService voucherService, IMapper mapper)
        {
            _voucherService = voucherService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherVM>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public IActionResult GetAllVoucher()
        {
            var result = _voucherService.GetAllVoucher();
            if(result.IsNullOrEmpty() || result == null)
            {
                return NotFound("No Voucher Found (Empty) !!!");
            }
            return Ok(_mapper.Map<IEnumerable<ResponseVoucherVM>>(result));
        }

        [HttpGet("{VoucherId}", Name = nameof(GetVoucherById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseVoucherVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetVoucherById(int VoucherId)
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherService.GetVoucherById(VoucherId);
                if(result == null)
                {
                    return NotFound("No Voucher Found with the given id !!!");
                }
                return Ok(_mapper.Map<ResponseVoucherVM>(result));
            }
            return BadRequest("Some properties are not valid !!!");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseVoucherVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
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
