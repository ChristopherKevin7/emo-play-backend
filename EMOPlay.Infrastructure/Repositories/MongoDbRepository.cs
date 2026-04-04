using System.Linq.Expressions;
using MongoDB.Driver;
using EMOPlay.Domain.Interfaces;

namespace EMOPlay.Infrastructure.Repositories;

public class MongoDbRepository<T> : IRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoDbRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return (int)await _collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
        
        return (int)await _collection.CountDocumentsAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public void Update(T entity)
    {
        var id = entity.GetType().GetProperty("Id")?.GetValue(entity) as Guid?;
        if (id == null) return;

        var filter = Builders<T>.Filter.Eq("_id", id.Value);
        _collection.ReplaceOne(filter, entity);
    }

    public void Delete(T entity)
    {
        var id = entity.GetType().GetProperty("Id")?.GetValue(entity) as Guid?;
        if (id == null) return;

        var filter = Builders<T>.Filter.Eq("_id", id.Value);
        _collection.DeleteOne(filter);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await _collection.Find(predicate).FirstOrDefaultAsync();
        return result != null;
    }
}
