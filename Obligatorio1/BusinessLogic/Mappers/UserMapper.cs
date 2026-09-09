using Domain;
using Domain.Enums;
using DTOs.User;

namespace BusinessLogic.Mappers;

public static class UserMapper
{
    public static UserSummaryDto ToSummaryDto(User user)
    {
        return new UserSummaryDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Role = user.Role
        };
    }

    public static User ToDomain(RegisterUserDto dto, Role role = Role.Client)
    {
        return new User(
            dto.Name,
            dto.Surname,
            dto.Email,
            dto.Phone,
            dto.Password,
            role
        );
    }

    public static User AdminToDomain(UpdateUserDto dto)
    {
        return new User(
            dto.Name,
            dto.Surname,
            dto.Email,
            dto.Phone,
            dto.Password,
            dto.Role);
    }
}
