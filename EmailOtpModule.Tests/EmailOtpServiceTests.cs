using EmailOtpCore.Constants;
using EmailOtpCore.Interfaces;
using EmailOtpCore.Models;
using EmailOtpCore.Services;
using Moq;

namespace EmailOtpModule.Tests;

public class EmailOtpServiceTests
{
    [Fact]
    public async Task GenerateOtpEmail_ValidEmail_ReturnsEmailOkAndOtp()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        emailSender
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.GenerateOtpEmailAsync("user@dso.org.sg");

        Assert.Equal(StatusCodes.STATUS_EMAIL_OK, result.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Otp));

        repo.Verify(x => x.AddAsync(It.IsAny<OtpRecord>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateOtpEmail_InvalidEmail_ReturnsEmailInvalid()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.GenerateOtpEmailAsync("user@gmail.com");

        Assert.Equal(StatusCodes.STATUS_EMAIL_INVALID, result.StatusCode);
        Assert.Null(result.Otp);

        repo.Verify(x => x.AddAsync(It.IsAny<OtpRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateOtpEmail_EmailSendFails_ReturnsEmailFail()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        emailSender
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.GenerateOtpEmailAsync("user@dso.org.sg");

        Assert.Equal(StatusCodes.STATUS_EMAIL_FAIL, result.StatusCode);
        Assert.Null(result.Otp);

        repo.Verify(x => x.AddAsync(It.IsAny<OtpRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyOtp_CorrectOtp_ReturnsOtpOk()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        repo.Setup(x => x.GetLatestByEmailAsync("user@dso.org.sg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpRecord
            {
                Email = "user@dso.org.sg",
                OtpCode = "123456",
                ExpiryUtc = DateTime.UtcNow.AddMinutes(1),
                FailedAttempts = 0,
                IsUsed = false,
                CreatedUtc = DateTime.UtcNow
            });

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.VerifyOtpAsync("user@dso.org.sg", "123456");

        Assert.Equal(StatusCodes.STATUS_OTP_OK, result);
        repo.Verify(x => x.UpdateAsync(It.Is<OtpRecord>(r => r.IsUsed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyOtp_WrongOtp_ReturnsOtpFail()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        repo.Setup(x => x.GetLatestByEmailAsync("user@dso.org.sg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpRecord
            {
                Email = "user@dso.org.sg",
                OtpCode = "123456",
                ExpiryUtc = DateTime.UtcNow.AddMinutes(1),
                FailedAttempts = 0,
                IsUsed = false,
                CreatedUtc = DateTime.UtcNow
            });

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.VerifyOtpAsync("user@dso.org.sg", "000000");

        Assert.Equal(StatusCodes.STATUS_OTP_FAIL, result);
        repo.Verify(x => x.UpdateAsync(It.Is<OtpRecord>(r => r.FailedAttempts == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyOtp_ExpiredOtp_ReturnsOtpTimeout()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        repo.Setup(x => x.GetLatestByEmailAsync("user@dso.org.sg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpRecord
            {
                Email = "user@dso.org.sg",
                OtpCode = "123456",
                ExpiryUtc = DateTime.UtcNow.AddSeconds(-1),
                FailedAttempts = 0,
                IsUsed = false,
                CreatedUtc = DateTime.UtcNow
            });

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.VerifyOtpAsync("user@dso.org.sg", "123456");

        Assert.Equal(StatusCodes.STATUS_OTP_TIMEOUT, result);
    }

    [Fact]
    public async Task VerifyOtp_AlreadyUsed_ReturnsOtpTimeout()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        repo.Setup(x => x.GetLatestByEmailAsync("user@dso.org.sg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpRecord
            {
                Email = "user@dso.org.sg",
                OtpCode = "123456",
                ExpiryUtc = DateTime.UtcNow.AddMinutes(1),
                FailedAttempts = 0,
                IsUsed = true,
                CreatedUtc = DateTime.UtcNow
            });

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.VerifyOtpAsync("user@dso.org.sg", "123456");

        Assert.Equal(StatusCodes.STATUS_OTP_TIMEOUT, result);
    }

    [Fact]
    public async Task VerifyOtp_TenFailedAttempts_ReturnsOtpFail()
    {
        var repo = new Mock<IOtpRepository>();
        var emailSender = new Mock<IEmailSender>();

        repo.Setup(x => x.GetLatestByEmailAsync("user@dso.org.sg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpRecord
            {
                Email = "user@dso.org.sg",
                OtpCode = "123456",
                ExpiryUtc = DateTime.UtcNow.AddMinutes(1),
                FailedAttempts = 10,
                IsUsed = false,
                CreatedUtc = DateTime.UtcNow
            });

        var service = new EmailOtpService(repo.Object, emailSender.Object);

        var result = await service.VerifyOtpAsync("user@dso.org.sg", "000000");

        Assert.Equal(StatusCodes.STATUS_OTP_FAIL, result);
    }
}