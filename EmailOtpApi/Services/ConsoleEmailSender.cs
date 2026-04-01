using System.Diagnostics;
using EmailOtpCore.Interfaces;

namespace EmailOtpApi.Services;

public class ConsoleEmailSender : IEmailSender
{
    public Task<bool> SendEmailAsync(string emailAddress, string emailBody, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine("==== Simulated Email Sender ====");
        Debug.WriteLine($"To: {emailAddress}");
        Debug.WriteLine($"Body: {emailBody}");
        Debug.WriteLine("================================");

        return Task.FromResult(true);
    }
}