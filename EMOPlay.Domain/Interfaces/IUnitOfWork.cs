using EMOPlay.Domain.Entities;

namespace EMOPlay.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Child> Children { get; }
    IRepository<Psychologist> Psychologists { get; }
    IRepository<GameSession> GameSessions { get; }
    IRepository<Challenge> Challenges { get; }
    IRepository<SessionResult> SessionResults { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
