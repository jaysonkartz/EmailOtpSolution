using EmailOtpCore.Models;

namespace EmailOtpCore.Interfaces;

public interface IOtpRepository
{
    Task AddAsync(OtpRecord record, CancellationToken cancellationToken = default);
    Task<OtpRecord?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateAsync(OtpRecord record, CancellationToken cancellationToken = default);
}