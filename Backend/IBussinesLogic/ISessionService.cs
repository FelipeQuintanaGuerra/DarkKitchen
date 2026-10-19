using DTOs.User;

namespace IBussinesLogic;

public interface ISessionService
{
    LoginResponseDto Login(LoginDto loginDto);
    void Logout(Guid token);
}