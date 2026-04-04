using EMOPlay.Domain.Entities;
using EMOPlay.Domain.Interfaces;
using EMOPlay.Infrastructure.Data;

namespace EMOPlay.Infrastructure.Repositories;

public class MongoDbUnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private IRepository<User>? _usersRepository;
    private IRepository<Child>? _childrenRepository;
    private IRepository<Psychologist>? _psychologistsRepository;
    private IRepository<GameSession>? _gameSessionsRepository;
    private IRepository<Challenge>? _challengesRepository;
    private IRepository<SessionResult>? _sessionResultsRepository;

    public MongoDbUnitOfWork(MongoDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _usersRepository ??= 
        new MongoDbRepository<User>(_context.Users);

    public IRepository<Child> Children => _childrenRepository ??= 
        new MongoDbRepository<Child>(_context.Children);
    
    public IRepository<Psychologist> Psychologists => _psychologistsRepository ??= 
        new MongoDbRepository<Psychologist>(_context.Psychologists);
    
    public IRepository<GameSession> GameSessions => _gameSessionsRepository ??= 
        new MongoDbRepository<GameSession>(_context.GameSessions);
    
    public IRepository<Challenge> Challenges => _challengesRepository ??= 
        new MongoDbRepository<Challenge>(_context.Challenges);
    
    public IRepository<SessionResult> SessionResults => _sessionResultsRepository ??= 
        new MongoDbRepository<SessionResult>(_context.SessionResults);

    public async Task<int> SaveChangesAsync()
    {
        // MongoDB não precisa de SaveChangesAsync explícito
        // As operações são executadas imediatamente
        return await Task.FromResult(1);
    }

    public async Task BeginTransactionAsync()
    {
        // MongoDB transactions precisam de replica set
        // Por enquanto, vamos deixar como stub
        await Task.CompletedTask;
    }

    public async Task CommitTransactionAsync()
    {
        // MongoDB transactions precisam de replica set
        await Task.CompletedTask;
    }

    public async Task RollbackTransactionAsync()
    {
        // MongoDB transactions precisam de replica set
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // MongoDB connections are handled by the driver
    }
}
