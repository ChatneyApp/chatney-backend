using System.Collections.Concurrent;
using System.Linq.Expressions;
using Npgsql;
using RepoDb;

namespace ChatneyBackend.Infra;

public interface IPgTimestamped
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IPgKey<T, in TKey> where T : class
{
    static abstract Expression<Func<T, bool>> MatchByKey(TKey key);
}

public class PgRepo<T, TKey> where T : class, IPgKey<T, TKey>
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

    public async Task<T?> GetById(TKey key)
    {
        return await GetOne(T.MatchByKey(key));
    }

    public async Task<T?> GetOne(Expression<Func<T, bool>> where)
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAsync(where, top: 1)).FirstOrDefault();
    }

    public async Task<List<T>> GetList()
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAllAsync<T>()).ToList();
    }

    public async Task<List<T>> GetList(Expression<Func<T, bool>> where)
    {
        await using var conn = await OpenAsync();
        return (await conn.QueryAsync(where)).ToList();
    }

    public async Task<TKey> InsertOne(T record)
    {
        await using var conn = await OpenAsync();
        return await conn.InsertAsync<T, TKey>(record);
    }

    public async Task InsertBulk(List<T> items)
    {
        if (items.Count == 0) return;
        await using var conn = await OpenAsync();
        await conn.InsertAllAsync(items);
    }

    public async Task<bool> DeleteById(TKey key)
    {
        return await Delete(T.MatchByKey(key)) > 0;
    }

    public async Task<long> Delete(Expression<Func<T, bool>> where)
    {
        await using var conn = await OpenAsync();
        return await conn.DeleteAsync(where);
    }

    public async Task<bool> UpdateOne(T record)
    {
        TouchUpdatedAt(record);
        await using var conn = await OpenAsync();
        return await conn.UpdateAsync(record) > 0;
    }

    public async Task UpdateBulk(List<T> items)
    {
        if (items.Count == 0) return;
        foreach (var item in items) TouchUpdatedAt(item);
        await using var conn = await OpenAsync();
        await conn.UpdateAllAsync(items);
    }

    public async Task Upsert(T record)
    {
        TouchUpdatedAt(record);
        await using var conn = await OpenAsync();
        await conn.MergeAsync(record);
    }

    public async Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object? param = null)
    {
        await using var conn = await OpenAsync();
        return await conn.ExecuteScalarAsync<TResult>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        await using var conn = await OpenAsync();
        return await conn.ExecuteNonQueryAsync(sql, param);
    }

    private static void TouchUpdatedAt(T record)
    {
        if (record is IPgTimestamped timestamped)
        {
            timestamped.UpdatedAt = DateTime.UtcNow;
        }
    }
}
