namespace EmailOtpCore.Models;

public class OtpRecord
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ExpiryUtc { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedUtc { get; set; }
}