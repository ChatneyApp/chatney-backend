using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Npgsql;
using RepoDb;
using RepoDb.Attributes;

namespace ChatneyBackend.Infra;

public interface IPgTimestamped
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class PgTimestamps
{
    public static void TouchForInsert(IPgTimestamped entity)
    {
        var now = DateTime.UtcNow;
        if (entity.CreatedAt == default)
        {
            entity.CreatedAt = now;
        }

        entity.UpdatedAt = now;
    }

    public static void TouchForUpdate(IPgTimestamped entity) =>
        entity.UpdatedAt = DateTime.UtcNow;
}

public interface IPgKey<T, TKey> where T : class
{
    static abstract Expression<Func<T, bool>> MatchByKey(TKey key);

    static abstract TKey GetKey(T record);
}

public class PgRepo<T, TKey> : IPgRepo<T, TKey> where T : class, IPgKey<T, TKey>
{
    private readonly NpgsqlDataSource _dataSource;

    private static readonly ConcurrentDictionary<Type, Lazy<bool>> _mappedTypes = new();

    /// <summary>
    /// The [Map] column names of every [Primary] property on T, computed once per closed generic
    /// type. Null for single-key entities (the common case, where RepoDb's default identity-column
    /// targeting is correct); non-null for composite-key entities (e.g. RoleAcl, UserAcl), where
    /// MergeAsync must be told explicitly which columns form the ON CONFLICT target - otherwise it
    /// only targets the first [Primary] property and Postgres rejects the upsert with 42P10.
    /// </summary>
    private static readonly Lazy<Field[]?> _compositeKeyQualifiers = new(BuildCompositeKeyQualifiers);

    private static Field[]? BuildCompositeKeyQualifiers()
    {
        var primaryColumns = typeof(T)
            .GetProperties()
            .Where(property => property.GetCustomAttribute<PrimaryAttribute>() != null)
            .Select(property => property.GetCustomAttribute<MapAttribute>()?.Name ?? property.Name)
            .ToArray();

        return primaryColumns.Length > 1 ? Field.From(primaryColumns).ToArray() : null;
    }

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
        if (record is IPgTimestamped timestamped)
        {
            PgTimestamps.TouchForInsert(timestamped);
        }

        await using var conn = await OpenAsync();

        if (_compositeKeyQualifiers.Value != null)
        {
            // Composite-key entities have no single identity column to return, so
            // InsertAsync<T, TKey> would try (and fail) to cast the generated identity into TKey.
            await conn.InsertAsync<T>(record);
            return T.GetKey(record);
        }

        return await conn.InsertAsync<T, TKey>(record);
    }

    public async Task InsertBulk(List<T> items)
    {
        if (items.Count == 0) return;

        foreach (var item in items)
        {
            if (item is IPgTimestamped timestamped)
            {
                PgTimestamps.TouchForInsert(timestamped);
            }
        }

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
        var qualifiers = _compositeKeyQualifiers.Value;

        if (qualifiers != null)
        {
            await conn.MergeAsync(record, qualifiers: qualifiers);
        }
        else
        {
            await conn.MergeAsync(record);
        }
    }

    public async Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object? param = null)
    {
        await using var conn = await OpenAsync();
        return await conn.ExecuteScalarAsync<TResult>(sql, param);
    }

    public async Task<TResult?> ExecuteScalarAsync<TResult>(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = await OpenAsync();
        await using var command = new NpgsqlCommand(sql, conn);
        command.Parameters.AddRange(parameters);

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return default;
        }

        return (TResult)result;
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
            PgTimestamps.TouchForUpdate(timestamped);
        }
    }
}
