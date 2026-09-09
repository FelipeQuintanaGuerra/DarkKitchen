namespace Domain;

using Domain.Enums;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;

public class User
{
    private static readonly string LocalUruguayPhonePattern = @"^09\d{7}$";
    private static readonly string InternationalPhonePattern = @"^\+[1-9]\d{6,14}$";

    public Guid Id { get; set; }
    public string Name { get; private set; }
    public string Surname { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string Password { get; private set; }
    public Role Role { get; private set; }
    public bool IsInternal { get; set; }
    public string Salt { get; private set; }
    public bool IsActive { get; set; }
    public List<Order> Orders { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public User() { }

    public User(string name, string surname, string email, string phone, string password, Role role, bool isInternal = false)
    {
        ValidateName(name);
        ValidateSurname(surname);
        ValidateEmail(email);
        ValidatePhone(phone);
        ValidatePassword(password);
        ValidateRole(role);
        Id = Guid.NewGuid();
        Name = name;
        Surname = surname;
        Email = email;
        Phone = phone;
        string salt = GenerateSalt();
        Salt = salt;
        Password = HashPassword(password, salt);
        Role = role;
        IsInternal = isInternal;
        IsActive = true;
    }

    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre no puede estar vacío.");
    }

    private void ValidateSurname(string surname)
    {
        if (string.IsNullOrWhiteSpace(surname))
            throw new ArgumentException("El apellido no puede estar vacío.");
        if (surname.Length < 3 || surname.Length > 25)
            throw new ArgumentException("El apellido debe tener entre 3 y 25 caracteres.");
    }

    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío.");

        const string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email, pattern))
            throw new ArgumentException("El formato de email tiene que se _@_._ .");
    }

    private void ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("El teléfono no puede estar vacío.");

        if (!Regex.IsMatch(phone, LocalUruguayPhonePattern) &&
            !Regex.IsMatch(phone, InternationalPhonePattern))
            throw new ArgumentException("El formato de teléfono Uruguayo debe ser 09_______");
    }

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía.");
        if (password.Length < 15 || password.Length > 25)
            throw new ArgumentException("La contraseña debe tener entre 15 y 25 caracteres.");
        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new ArgumentException("La contraseña debe tener al menos una letra mayúscula.");
        if (!Regex.IsMatch(password, "[a-z]"))
            throw new ArgumentException("La contraseña debe tener al menos una letra minúscula.");
        if (!Regex.IsMatch(password, "[\\W_]"))
            throw new ArgumentException("La contraseña debe tener al menos un símbolo.");
        if (!Regex.IsMatch(password, "\\d"))
            throw new ArgumentException("La contraseña debe tener al menos un número.");
        if (HasNumericSequence(password))
            throw new ArgumentException("La contraseña no debe tener secuencias numéricas.");
    }

    private static bool HasNumericSequence(string password)
    {
        for (int i = 0; i < password.Length - 3; i++)
        {
            if (!char.IsDigit(password[i])) continue;
            if (!char.IsDigit(password[i + 1])) continue;
            if (!char.IsDigit(password[i + 2])) continue;
            if (!char.IsDigit(password[i + 3])) continue;

            int d1 = password[i] - '0';
            int d2 = password[i + 1] - '0';
            int d3 = password[i + 2] - '0';
            int d4 = password[i + 3] - '0';

            bool ascending = d2 == (d1 + 1) % 10 && d3 == (d2 + 1) % 10 && d4 == (d3 + 1) % 10;
            bool descending = d2 == (d1 + 9) % 10 && d3 == (d2 + 9) % 10 && d4 == (d3 + 9) % 10;

            if (ascending || descending) return true;
        }
        return false;
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password)
    {
        var hashToCheck = HashPassword(password, Salt);
        return hashToCheck == Password;
    }

    public void Update(string name, string surname, string email, string phone, Role role, bool isInternal, bool isActive)
    {
        ValidateName(name);
        ValidateSurname(surname);
        ValidateEmail(email);
        ValidatePhone(phone);
        ValidateRole(role);

        Name = name;
        Surname = surname;
        Email = email;
        Phone = phone;
        Role = role;
        IsInternal = isInternal;
        IsActive = isActive;
    }

    private void ValidateRole(Role role)
    {
        if (!Enum.IsDefined(typeof(Role), role))
            throw new ArgumentException("El rol no es válido.");
    }
}