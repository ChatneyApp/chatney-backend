using System.Linq.Expressions;
using Npgsql;

namespace ChatneyBackend.Infra;

public interface IPgRepo<T, TKey> where T : class, IPgKey<T, TKey>
{
    Task<T?> GetById(TKey key);
    Task<T?> GetOne(Expression<Func<T, bool>> where);
    Task<List<T>> GetList();
    Task<List<T>> GetList(Expression<Func<T, bool>> where);
    Task<TKey> InsertOne(T record);
    Task InsertBulk(List<T> items);
    Task<bool> DeleteById(TKey key);
    Task<long> Delete(Expression<Func<T, bool>> where);
    Task<bool> UpdateOne(T record);
    Task UpdateBulk(List<T> items);
    Task Upsert(T record);
    Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object? param = null);
    Task<TResult?> ExecuteScalarAsync<TResult>(string sql, params NpgsqlParameter[] parameters);
    Task<int> ExecuteAsync(string sql, object? param = null);
}
