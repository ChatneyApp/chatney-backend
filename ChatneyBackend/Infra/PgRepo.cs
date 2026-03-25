using System.Collections.Concurrent;
using System.Linq.Expressions;
using Npgsql;
using RepoDb;

namespace ChatneyBackend.Infra;

public interface IPgDatabaseItem<T>
{
    public T Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PgRepo<T, TI> where T : class, IPgDatabaseItem<TI>
{
    private readonly NpgsqlDataSource _dataSource;

    private static readonly ConcurrentDictionary<Type, Lazy<bool>> _mappedTypes = new();

    public PgRepo(NpgsqlDataSource dataSource, string tableName)
    {
        _dataSource = dataSource;
        _ = _mappedTypes.GetOrAdd(typeof(T), _ => new Lazy<bool>(() =>
        {
            FluentMapper.Entity<T>().Table(tableName);
            return true;
        })).Value;
    }

    private async Task<NpgsqlConnection> OpenAsync() => await _dataSource.OpenConnectionAsync();

    public async Task<T?> GetById(TI id)
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAsync<T>(id)).FirstOrDefault();
    }

    public async Task<T?> GetOne(Expression<Func<T, bool>> where)
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAsync<T>(where, top: 1)).FirstOrDefault();
    }

    public async Task<List<T>> GetList()
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAllAsync<T>()).ToList();
    }

    public async Task<List<T>> GetList(Expression<Func<T, bool>> where)
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAsync<T>(where)).ToList();
    }

    public async Task<TI> InsertOne(T record)
    {
        await using var conn = await OpenAsync();
        return await conn.InsertAsync<T, TI>(record);
    }

    public async Task InsertBulk(List<T> items)
    {
        if (items.Count == 0) return;
        await using var conn = await OpenAsync();
        await conn.InsertAllAsync<T>(items);
    }

    public async Task<bool> DeleteById(TI id)
    {
        await using var conn = await OpenAsync();
        return await conn.DeleteAsync<T>(id) > 0;
    }

    public async Task<long> Delete(Expression<Func<T, bool>> where)
    {
        await using var conn = await OpenAsync();
        return await conn.DeleteAsync<T>(where);
    }

    public async Task<bool> UpdateOne(T record)
    {
        record.UpdatedAt = DateTime.UtcNow;
        await using var conn = await OpenAsync();
        return await conn.UpdateAsync<T>(record) > 0;
    }

    public async Task UpdateBulk(List<T> items)
    {
        if (items.Count == 0) return;
        foreach (var item in items) item.UpdatedAt = DateTime.UtcNow;
        await using var conn = await OpenAsync();
        await conn.UpdateAllAsync<T>(items);
    }

    public async Task<TI> Upsert(T record)
    {
        record.UpdatedAt = DateTime.UtcNow;
        await using var conn = await OpenAsync();
        return await conn.MergeAsync<T, TI>(record);
    }
}
