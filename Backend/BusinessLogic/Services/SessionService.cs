using BusinessLogic.Mappers;
using Domain;
using Domain.Enums;
using DTOs.User;
using IBussinesLogic;
using IRepository;

namespace BusinessLogic;

public class SessionService : ISessionService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ISessionRepository _sessionRepository;

    public SessionService(IGenericRepository<User> userRepository, ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    public LoginResponseDto Login(LoginDto dto)
    {
        ValidateDto(dto);

        var user = _userRepository.GetBy(u => u.Email == dto.Email);

        VerifyUser(user);
        ValidatePassword(dto, user);

        var session = new Session(user);
        _sessionRepository.Add(session);

        return new LoginResponseDto
        {
            Token = session.Token,
            User = UserMapper.ToSummaryDto(user)
        };
    }

    public void Logout(Guid token)
    {
        _sessionRepository.Remove(token);
    }

    private static void ValidatePassword(LoginDto dto, User? user)
    {
        if (!user.VerifyPassword(dto.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas.");
    }

    private static void VerifyUser(User? user)
    {
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("El usuario no existe o fue borrado.");
    }

    private static void ValidateDto(LoginDto dto)
    {
        if (dto == null ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("El mail y la contraseña son requeridos.");
        }
    }
}