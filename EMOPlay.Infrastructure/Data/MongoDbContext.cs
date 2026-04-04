using MongoDB.Driver;
using EMOPlay.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace EMOPlay.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoConnection") ?? 
            "mongodb://localhost:27017";
        var databaseName = configuration["MongoDBSettings:DatabaseName"] ?? "emoplay";
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Child> Children => _database.GetCollection<Child>("Children");
    public IMongoCollection<Psychologist> Psychologists => _database.GetCollection<Psychologist>("Psychologists");
    public IMongoCollection<GameSession> GameSessions => _database.GetCollection<GameSession>("GameSessions");
    public IMongoCollection<Challenge> Challenges => _database.GetCollection<Challenge>("Challenges");
    public IMongoCollection<SessionResult> SessionResults => _database.GetCollection<SessionResult>("SessionResults");

    public async Task InitializeAsync()
    {
        // Criar índices se precisar
        var usersCollection = Users;
        var usersEmailOptions = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true });
        
        try
        {
            await usersCollection.Indexes.CreateOneAsync(usersEmailOptions);
        }
        catch
        {
            // Índice já existe
        }

        var childrenCollection = Children;
        var childrenOptions = new CreateIndexModel<Child>(
            Builders<Child>.IndexKeys.Ascending(c => c.PsychologistId));
        
        try
        {
            await childrenCollection.Indexes.CreateOneAsync(childrenOptions);
        }
        catch
        {
            // Índice já existe
        }

        var gameSessionsCollection = GameSessions;
        var gameSessionsOptions = new CreateIndexModel<GameSession>(
            Builders<GameSession>.IndexKeys.Ascending(gs => gs.ChildId));
        
        try
        {
            await gameSessionsCollection.Indexes.CreateOneAsync(gameSessionsOptions);
        }
        catch
        {
            // Índice já existe
        }

        var challengesCollection = Challenges;
        var challengesOptions = new CreateIndexModel<Challenge>(
            Builders<Challenge>.IndexKeys.Ascending(c => c.SessionId));
        
        try
        {
            await challengesCollection.Indexes.CreateOneAsync(challengesOptions);
        }
        catch
        {
            // Índice já existe
        }

        var sessionResultsCollection = SessionResults;
        var sessionResultsOptions = new CreateIndexModel<SessionResult>(
            Builders<SessionResult>.IndexKeys.Ascending(sr => sr.SessionId));
        
        try
        {
            await sessionResultsCollection.Indexes.CreateOneAsync(sessionResultsOptions);
        }
        catch
        {
            // Índice já existe
        }
    }
}
