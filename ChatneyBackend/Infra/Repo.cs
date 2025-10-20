using MongoDB.Driver;
using System.Linq.Expressions;

public interface DatabaseItem
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Repo<T> where T : DatabaseItem
{
    private readonly IMongoCollection<T> _collection;

    public IMongoCollection<T> Collection => _collection;

    public Repo(IMongoDatabase db, string collectionName)
    {
        _collection = db.GetCollection<T>(collectionName);
    }

    public Task<T?> GetById(string id) => _collection.Find(u => u.Id == id).FirstOrDefaultAsync();

    public Task<T?> GetOne(Expression<Func<T, bool>> filter) => _collection.Find(filter).FirstOrDefaultAsync();

    public Task<List<T>> GetList(FilterDefinition<T> filter) => _collection.Find(filter).ToListAsync();

    public Task InsertOne(T record) => _collection.InsertOneAsync(record);

    public Task InsertBulk(List<T> items) => _collection.InsertManyAsync(items);

    public async Task<bool> DeleteById(string id) => await Delete(Builders<T>.Filter.Eq(r => r.Id, id)) > 0;

    public async Task<long> Delete(FilterDefinition<T> filter)
    {
        var r = await _collection.DeleteManyAsync(filter);
        return r.DeletedCount;
    }

    public async Task<bool> UpdateOne(T record)
    {
        record.UpdatedAt = DateTime.UtcNow;
        var r = await _collection.ReplaceOneAsync(u => u.Id == record.Id, record);
        return r.ModifiedCount > 0;
    }
    }
