using BusinessLogic.Mappers;
using Domain;
using Domain.Enums;
using DTOs.User;
using IBussinesLogic;
using IRepository;
namespace BusinessLogic;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ISessionRepository _sessionRepository;

    public UserService(IGenericRepository<User> userRepository, ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    public UserSummaryDto Create(UpdateUserDto dto, Guid token)
    {
        EnsureEmailIsAvailable(dto.Email);

        var user = UserMapper.AdminToDomain(dto);

        _userRepository.Add(user);

        return UserMapper.ToSummaryDto(user);
    }

    public UserSummaryDto Register(RegisterUserDto dto)
    {
        if (_userRepository.GetBy(u => u.Email == dto.Email) != null)
            throw new ArgumentException("Ya existe un usuario con este mail.");

        var user = UserMapper.ToDomain(dto);
        _userRepository.Add(user);

        return UserMapper.ToSummaryDto(user);
    }

    public void Update(UpdateUserDto dto, Guid token)
    {
        var sessionUser = GetCurrentUser(token);

        EnsureUserIsNotCurrentUser(dto.Id, sessionUser.Id);

        var user = GetUserOrThrow(dto.Id);

        user.Update(dto.Name, dto.Surname, dto.Email, dto.Phone, dto.Role, dto.IsInternal, dto.IsActive);

        EnsureEmailIsAvailableForUpdate(dto.Email, dto.Id);

        _userRepository.Update(user);
    }

    public void Delete(Guid id, Guid token)
    {
        var sessionUser = GetCurrentUser(token);

        EnsureUserIsNotCurrentUser(id, sessionUser.Id);

        var user = GetUserOrThrow(id);

        user.IsActive = false;

        _userRepository.Update(user);
    }

    public List<UserSummaryDto> SearchBy(string name, string surname)
    {
        var nameFilter = (name ?? "").Trim().ToLower();
        var surnameFilter = (surname ?? "").Trim().ToLower();

        var users = _userRepository.GetAllBy(u =>
            (nameFilter == "" || u.Name.ToLower().Contains(nameFilter)) &&
            (surnameFilter == "" || u.Surname.ToLower().Contains(surnameFilter)));

        return users.Select(UserMapper.ToSummaryDto).ToList();
    }

    public UserSummaryDto? GetUserById(Guid id)
    {
        var user = _userRepository.GetBy(u => u.Id == id);
        if (user == null)
            throw new KeyNotFoundException("No existe un usuario con ese ID.");

        return UserMapper.ToSummaryDto(user);
    }

    private User GetCurrentUser(Guid token)
    {
        var session = _sessionRepository.GetByToken(token);

        if (session == null)
            throw new UnauthorizedAccessException();

        return session.User;
    }

    private void EnsureEmailIsAvailable(string email)
    {
        var existing = _userRepository.GetBy(u => u.Email == email);

        if (existing != null)
            throw new ConflictException("Ya existe un usuario con ese email.");
    }

    private void EnsureEmailIsAvailableForUpdate(string email, Guid userId)
    {
        var existing = _userRepository.GetAllBy(u => u.Email == email && u.Id != userId);

        if (existing.Any())
            throw new ConflictException("Ya existe un usuario con ese email.");
    }

    private User GetUserOrThrow(Guid id)
    {
        var user = _userRepository.GetBy(u => u.Id == id);

        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        return user;
    }
    private static void EnsureUserIsNotCurrentUser(Guid targetUserId, Guid currentUserId)
    {
        if (targetUserId == currentUserId)
            throw new InvalidOperationException("El usuario no puede modificarse a sí mismo.");
    }

}