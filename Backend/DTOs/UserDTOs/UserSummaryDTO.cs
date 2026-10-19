using Domain.Enums;

namespace DTOs.User;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}