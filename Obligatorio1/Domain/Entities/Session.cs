namespace Domain;

public class Session
{
    public Guid Token { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    private Session() { }
    public Session(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        Token = Guid.NewGuid();
        UserId = user.Id;
        User = user;
    }
}