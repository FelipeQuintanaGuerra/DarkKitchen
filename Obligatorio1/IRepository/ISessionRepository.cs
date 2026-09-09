using Domain;

namespace IRepository;

public interface ISessionRepository
{
    public void Add(Session session);
    public Session? GetByToken(Guid token);
    public void Remove(Guid token);
}