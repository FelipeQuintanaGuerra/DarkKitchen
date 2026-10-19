using Domain;
using IRepository;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Session session)
    {
        _context.Entry(session.User).State = EntityState.Unchanged;
        _context.Sessions.Add(session);
        _context.SaveChanges();
    }

    public Session? GetByToken(Guid token)
    {
        return _context.Sessions
            .Include(s => s.User)
            .FirstOrDefault(s => s.Token == token);
    }

    public void Remove(Guid token)
    {
        var session = _context.Sessions.FirstOrDefault(s => s.Token == token);
        if (session != null)
        {
            _context.Sessions.Remove(session);
            _context.SaveChanges();
        }
    }
}