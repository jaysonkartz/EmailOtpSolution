using EmailOtpCore.Interfaces;
using EmailOtpCore.Models;
using EmailOtpInfrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmailOtpInfrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _dbContext;

    public OtpRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OtpRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.OtpRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OtpRecord?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.OtpRecords
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(OtpRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.OtpRecords.Update(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}