using EmailOtpApi.Dtos;
using OtpStatusCodes = EmailOtpCore.Constants.StatusCodes;
using EmailOtpCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailOtpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly EmailOtpService _emailOtpService;

    public OtpController(EmailOtpService emailOtpService)
    {
        _emailOtpService = emailOtpService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _emailOtpService.GenerateOtpEmailAsync(request.Email, cancellationToken);

        if (result.StatusCode == OtpStatusCodes.STATUS_EMAIL_OK)
        {
            return Ok(new
            {
                statusCode = result.StatusCode,
                message = "OTP generated successfully",
                otp = result.Otp
            });
        }

        if (result.StatusCode == OtpStatusCodes.STATUS_EMAIL_INVALID)
        {
            return BadRequest(new
            {
                statusCode = result.StatusCode,
                message = "Email address is invalid."
            });
        }

        return BadRequest(new
        {
            statusCode = result.StatusCode,
            message = "Email address does not exist or sending to the email has failed."
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        int result = await _emailOtpService.VerifyOtpAsync(request.Email, request.Otp, cancellationToken);

        if (result == OtpStatusCodes.STATUS_OTP_OK)
        {
            return Ok(new
            {
                statusCode = result,
                message = "OTP is valid and checked"
            });
        }

        if (result == OtpStatusCodes.STATUS_OTP_TIMEOUT)
        {
            return BadRequest(new
            {
                statusCode = result,
                message = "Timeout after 1 min"
            });
        }

        return BadRequest(new
        {
            statusCode = result,
            message = "OTP is invalid"
        });
    }
}