using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers
{
    [Authorize(Policy = "VoucherExtension")]
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherExtensionsController : ControllerBase
    {
        private readonly IExpiredDateExtensionService _expiredDateExtensionService;
        private readonly IMapper _mapper;

        public VoucherExtensionsController(IExpiredDateExtensionService expiredDateExtensionService, IMapper mapper)
        {
            _expiredDateExtensionService = expiredDateExtensionService;
            _mapper = mapper;
        }

        [Authorize(Policy = "All")]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        public IActionResult GetAllExpiredDateExtensions()
        {
            var result = _expiredDateExtensionService.GetAllExpiredDateExtensions();
            return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
        }

        [Authorize(Policy = "All")]
        [HttpGet("all-delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        public IActionResult GetAllDeletedExpiredDateExtensions()
        {
            var result = _expiredDateExtensionService.GetAllDeletedExpiredDateExtensions();
            return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
        }

        [Authorize(Policy = "Read")]
        [HttpGet("{extensionId}", Name = nameof(GetExpiredDateExtensionById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseExpiredDateExtensionVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetExpiredDateExtensionById(int extensionId)
        {
            if (ModelState.IsValid)
            {
                var result = await _expiredDateExtensionService.GetExpiredDateExtensionById(extensionId);
                if (result == null)
                {
                    return NotFound( new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn gia hạn voucher",
                        Error = new List<string>() { "Can not find voucher extension with the given id: " + extensionId }
                    });
                }
                return Ok(_mapper.Map<ResponseExpiredDateExtensionVM>(result));
            }
            return BadRequest( new ServiceResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Error = new List<string>() { "Invalid input" }
            });
        }

        [Authorize(Policy = "Read")]
        [HttpGet("User")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetAllExpiredDateExtensionOfUser()
        {
            try
            {
                var result = await _expiredDateExtensionService.GetAllExpiredDateExtensionOfUser();
                return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
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

        [Authorize(Policy = "Read")]
        [HttpGet("User/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetAllExpiredDateExtensionOfUserById(Guid userId)
        {
            try
            {
                var result = await _expiredDateExtensionService.GetAllExpiredDateExtensionOfUserById(userId);
                return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetAllExpiredDateExtensionOfCustomer()
        {
            try
            {
                var result = await _expiredDateExtensionService.GetAllExpiredDateExtensionOfCustomer();
                return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
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

        [Authorize(Policy = "Read")]
        [HttpGet("Customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetAllExpiredDateExtensionOfCustomer(Guid customerId)
        {
            try
            {
                var result = await _expiredDateExtensionService.GetAllExpiredDateExtensionOfCustomerById(customerId);
                return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
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

        [Authorize(Policy = "Write")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseExpiredDateExtensionVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> AddNewExpiredDateExtension([FromBody] ExpiredDateExtensionVM resource)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.AddNewExpiredDateExtension(_mapper.Map<ExpiredDateExtension>(resource), resource.VoucherId);
                    if (result != null)
                    {
                        return CreatedAtAction(nameof(GetExpiredDateExtensionById),
                            new
                            {
                                extensionId = result.Id
                            },
                            _mapper.Map<ResponseExpiredDateExtensionVM>(result));
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
                    Message = "Dữ liệu không hợp lệ",
                    Error = new List<string>() { "Invalid input" }
                });
            }
            catch (ArgumentNullException ex)
            {
                var messages = ex.Message.Split(":");
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = messages.First(),
                    Error = new List<string> { "Can not find with the given id: " + messages.Last() }
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
                    Message = "Gia hạn thất bại",
                    Error = new List<string>() { ex.Message }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Gia hạn thất bại",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        [Authorize(Policy = "Update")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> UpdateInformation(int id, [FromBody] UpdatedExpiredDateExtensionVM resource)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.UpdateVoucherExtension(resource, id);
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
                    Message = "Dữ liệu không hợp lệ",
                    Error = new List<string>() { "Invalid input" }
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
        public async Task<IActionResult> SoftDeleteVoucherExtension(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.SoftDelete(id);
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

        [Authorize(Policy = "Restore")]
        [HttpPatch("restore-voucherextension/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> RestoreVoucherExtension(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.RestoreVoucherExtension(id);
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
                    Message = "Khôi phục thất bại",
                    Error = new List<string>() { ex.Message }
                });
            }
        }
    }
}
