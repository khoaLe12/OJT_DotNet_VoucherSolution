using Base.API.Services;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

	public MailController(IMailService mailService)
	{
		_mailService = mailService;
	}

	[HttpPost("Test-Mail")]
	[SwaggerOperation("Provide Mail address (required). Other like: subject, content, and alo uploads multiple files (options).")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> SendMailTest([FromForm] MailMessageVM resource)
	{
		//var files = Request.Form.Files.Any() ? Request.Form.Files : new FormFileCollection();

		if (ModelState.IsValid)
		{
			var message = new Message
			{
				To = resource.To,
				Subject = resource.Subject,
				Content = resource.Content,
				Attachments = resource.Files,
			};
			await _mailService.SendMailAsync(message);
			return Ok();
		}
		else
		{
			return BadRequest("Invalid input");
		}
		
    }
}
