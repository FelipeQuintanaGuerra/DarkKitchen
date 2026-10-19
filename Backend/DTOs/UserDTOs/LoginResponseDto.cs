namespace DTOs.User;

public class LoginResponseDto
{
    public Guid Token { get; set; }
    public UserSummaryDto User { get; set; }
}