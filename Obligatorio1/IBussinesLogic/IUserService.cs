using DTOs.User;

namespace IBussinesLogic;

public interface IUserService
{
    public UserSummaryDto Create(UpdateUserDto dto, Guid token);
    UserSummaryDto Register(RegisterUserDto dto);
    public void Update(UpdateUserDto dto, Guid token);
    public void Delete(Guid id, Guid token);
    public List<UserSummaryDto> SearchBy(string name, string surname);
    public UserSummaryDto? GetUserById(Guid id);
}