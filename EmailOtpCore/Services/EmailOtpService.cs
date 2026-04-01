using System.Net.Mail;
using System.Security.Cryptography;
using EmailOtpCore.Constants;
using EmailOtpCore.Interfaces;
using EmailOtpCore.Models;

namespace EmailOtpCore.Services;

public class EmailOtpService
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(1);

    private readonly IOtpRepository _otpRepository;
    private readonly IEmailSender _emailSender;

    public EmailOtpService(IOtpRepository otpRepository, IEmailSender emailSender)
    {
        _otpRepository = otpRepository;
        _emailSender = emailSender;
    }

    public async Task<(int StatusCode, string? Otp)> GenerateOtpEmailAsync(string userEmail, CancellationToken cancellationToken = default)
    {
        if (!IsAllowedEmail(userEmail))
            return (StatusCodes.STATUS_EMAIL_INVALID, null);

        string otp = GenerateOtp();
        string emailBody = $"You OTP Code is {otp}. The code is valid for 1 minute";

        var record = new OtpRecord
        {
            Email = userEmail.Trim(),
            OtpCode = otp,
            ExpiryUtc = DateTime.UtcNow.Add(OtpValidity),
            FailedAttempts = 0,
            IsUsed = false,
            CreatedUtc = DateTime.UtcNow
        };

        bool sent;
        try
        {
            sent = await _emailSender.SendEmailAsync(record.Email, emailBody, cancellationToken);
        }
        catch
        {
            sent = false;
        }

        if (!sent)
            return (StatusCodes.STATUS_EMAIL_FAIL, null);

        await _otpRepository.AddAsync(record, cancellationToken);

        return (StatusCodes.STATUS_EMAIL_OK, otp);
    }

    public async Task<int> VerifyOtpAsync(string email, string inputOtp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(inputOtp))
            return StatusCodes.STATUS_OTP_FAIL;

        var record = await _otpRepository.GetLatestByEmailAsync(email.Trim(), cancellationToken);

        if (record is null || record.IsUsed)
            return StatusCodes.STATUS_OTP_TIMEOUT;

        if (DateTime.UtcNow > record.ExpiryUtc)
            return StatusCodes.STATUS_OTP_TIMEOUT;

        if (record.FailedAttempts >= MaxAttempts)
            return StatusCodes.STATUS_OTP_FAIL;

        if (string.Equals(record.OtpCode, inputOtp.Trim(), StringComparison.Ordinal))
        {
            record.IsUsed = true;
            await _otpRepository.UpdateAsync(record, cancellationToken);
            return StatusCodes.STATUS_OTP_OK;
        }

        record.FailedAttempts += 1;
        await _otpRepository.UpdateAsync(record, cancellationToken);

        return StatusCodes.STATUS_OTP_FAIL;
    }

    private static bool IsAllowedEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email.Trim());
            string domain = addr.Host.ToLowerInvariant();

            return domain == "dso.org.sg" || domain.EndsWith(".dso.org.sg", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateOtp()
    {
        int value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }
}