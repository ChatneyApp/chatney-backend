using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Infra;
using Npgsql;

namespace ChatneyBackend.Tests.Support;

public class InMemoryPgRepo<T, TKey> : IPgRepo<T, TKey> where T : class, IPgKey<T, TKey>
{
    private readonly List<T> _items = [];
    private readonly object _lock = new();

    public IReadOnlyList<T> Items
    {
        get
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }
    }

    public void Seed(params T[] items)
    {
        lock (_lock)
        {
            _items.AddRange(items);
        }
    }

    public Task<T?> GetById(TKey key) => Task.FromResult(GetOneSync(T.MatchByKey(key)));

    public Task<T?> GetOne(Expression<Func<T, bool>> where) => Task.FromResult(GetOneSync(where));

    public Task<List<T>> GetList()
    {
        lock (_lock)
        {
            return Task.FromResult(_items.ToList());
        }
    }

    public Task<List<T>> GetList(Expression<Func<T, bool>> where)
    {
        lock (_lock)
        {
            var predicate = where.Compile();
            return Task.FromResult(_items.Where(predicate).ToList());
        }
    }

    public Task<TKey> InsertOne(T record)
    {
        lock (_lock)
        {
            AssignKeyIfNeeded(record);
            _items.Add(record);
            return Task.FromResult(ReadKey(record));
        }
    }

    public Task InsertBulk(List<T> items)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                AssignKeyIfNeeded(item);
                _items.Add(item);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteById(TKey key) =>
        Task.FromResult(DeleteSync(T.MatchByKey(key)) > 0);

    public Task<long> Delete(Expression<Func<T, bool>> where) =>
        Task.FromResult(DeleteSync(where));

    public Task<bool> UpdateOne(T record)
    {
        lock (_lock)
        {
            var key = ReadKey(record);
            var index = _items.FindIndex(item => EqualityComparer<TKey>.Default.Equals(ReadKey(item), key));
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            TouchUpdatedAt(record);
            _items[index] = record;
            return Task.FromResult(true);
        }
    }

    public Task UpdateBulk(List<T> items)
    {
        foreach (var item in items)
        {
            UpdateOne(item);
        }

        return Task.CompletedTask;
    }

    public Task Upsert(T record)
    {
        lock (_lock)
        {
            var key = ReadKey(record);
            var index = _items.FindIndex(item => EqualityComparer<TKey>.Default.Equals(ReadKey(item), key));
            TouchUpdatedAt(record);
            if (index < 0)
            {
                AssignKeyIfNeeded(record);
                _items.Add(record);
            }
            else
            {
                _items[index] = record;
            }
        }

        return Task.CompletedTask;
    }

    public Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object? param = null)
    {
        if (typeof(T) == typeof(Message))
        {
            return Task.FromResult(ExecuteMessageScalar<TResult>(sql, param));
        }

        throw new NotSupportedException($"ExecuteScalarAsync is not supported for {typeof(T).Name}");
    }

    public Task<TResult?> ExecuteScalarAsync<TResult>(string sql, params NpgsqlParameter[] parameters)
    {
        if (typeof(T) == typeof(Message))
        {
            return Task.FromResult(ExecuteMessageScalar<TResult>(sql, parameters));
        }

        throw new NotSupportedException($"ExecuteScalarAsync is not supported for {typeof(T).Name}");
    }

    public Task<int> ExecuteAsync(string sql, object? param = null)
    {
        if (typeof(T) == typeof(MessageReaction))
        {
            return Task.FromResult(ExecuteReactionNonQuery(sql, param));
        }

        throw new NotSupportedException($"ExecuteAsync is not supported for {typeof(T).Name}");
    }

    private T? GetOneSync(Expression<Func<T, bool>> where)
    {
        lock (_lock)
        {
            var predicate = where.Compile();
            return _items.FirstOrDefault(predicate);
        }
    }

    private long DeleteSync(Expression<Func<T, bool>> where)
    {
        lock (_lock)
        {
            var predicate = where.Compile();
            var toRemove = _items.Where(predicate).ToList();
            foreach (var item in toRemove)
            {
                _items.Remove(item);
            }

            return toRemove.Count;
        }
    }

    private void AssignKeyIfNeeded(T record)
    {
        if (typeof(TKey) == typeof(int))
        {
            var id = (int)(object)ReadKey(record)!;
            if (id == 0)
            {
                var nextId = _items.Count == 0
                    ? 1
                    : _items.Select(item => (int)(object)ReadKey(item)!).Max() + 1;
                SetKey(record, (TKey)(object)nextId);
            }
        }
    }

    private static TKey ReadKey(T record)
    {
        if (typeof(T) == typeof(MessageReaction))
        {
            var reaction = (MessageReaction)(object)record;
            return (TKey)(object)new MessageReactionKey(reaction.MessageId, reaction.UserId, reaction.Code);
        }

        var idProperty = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have an Id property.");

        return (TKey)idProperty.GetValue(record)!;
    }

    private static void SetKey(T record, TKey key)
    {
        if (typeof(T) == typeof(MessageReaction))
        {
            return;
        }

        var idProperty = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have an Id property.");

        idProperty.SetValue(record, key);
    }

    private static void TouchUpdatedAt(T record)
    {
        if (record is IPgTimestamped timestamped)
        {
            timestamped.UpdatedAt = DateTime.UtcNow;
        }
    }

    private TResult? ExecuteMessageScalar<TResult>(string sql, object? param)
    {
        var messageId = ReadAnonymousInt(param, "Id");
        lock (_lock)
        {
            var message = _items
                .Cast<Message>()
                .FirstOrDefault(item => item.Id == messageId);

            if (message == null)
            {
                return default;
            }

            if (sql.Contains("children_count + 1", StringComparison.Ordinal))
            {
                message.ChildrenCount += 1;
                message.UpdatedAt = DateTime.UtcNow;
                return (TResult)(object)message.ChildrenCount;
            }

            if (sql.Contains("GREATEST(children_count - 1, 0)", StringComparison.Ordinal))
            {
                message.ChildrenCount = Math.Max(message.ChildrenCount - 1, 0);
                message.UpdatedAt = DateTime.UtcNow;
                return (TResult)(object)message.ChildrenCount;
            }

            throw new NotSupportedException($"Unsupported message SQL: {sql}");
        }
    }

    private TResult? ExecuteMessageScalar<TResult>(string sql, NpgsqlParameter[] parameters)
    {
        var messageId = ReadParameter<int>(parameters, "Id");
        var content = ReadParameter<string>(parameters, "Content");
        var attachmentIds = ReadParameter<int[]>(parameters, "AttachmentIds") ?? [];
        var urlPreviewIds = ReadParameter<int[]>(parameters, "UrlPreviewIds") ?? [];

        lock (_lock)
        {
            var message = _items
                .Cast<Message>()
                .FirstOrDefault(item => item.Id == messageId);

            if (message == null)
            {
                return default;
            }

            if (sql.Contains("SET content = @Content", StringComparison.Ordinal))
            {
                message.Content = content;
                message.AttachmentIds = attachmentIds;
                message.UrlPreviewIds = urlPreviewIds;
                message.UpdatedAt = DateTime.UtcNow;
                return (TResult)(object)message.UpdatedAt;
            }

            throw new NotSupportedException($"Unsupported message SQL: {sql}");
        }
    }

    private int ExecuteReactionNonQuery(string sql, object? param)
    {
        var messageId = ReadAnonymousInt(param, "MessageId");
        var userId = ReadAnonymousGuid(param, "UserId");
        var code = ReadAnonymousString(param, "Code") ?? string.Empty;

        lock (_lock)
        {
            if (sql.Contains("INSERT INTO message_reactions", StringComparison.Ordinal))
            {
                var exists = _items
                    .Cast<MessageReaction>()
                    .Any(reaction =>
                        reaction.MessageId == messageId &&
                        reaction.UserId == userId &&
                        reaction.Code == code);

                if (exists)
                {
                    return 0;
                }

                _items.Add((T)(object)new MessageReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    Code = code,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
                return 1;
            }

            if (sql.Contains("DELETE FROM message_reactions", StringComparison.Ordinal))
            {
                var reaction = _items
                    .Cast<MessageReaction>()
                    .FirstOrDefault(item =>
                        item.MessageId == messageId &&
                        item.UserId == userId &&
                        item.Code == code);

                if (reaction == null)
                {
                    return 0;
                }

                _items.Remove((T)(object)reaction);
                return 1;
            }

            throw new NotSupportedException($"Unsupported reaction SQL: {sql}");
        }
    }

    private static int ReadAnonymousInt(object? param, string propertyName)
    {
        if (param == null)
        {
            throw new ArgumentNullException(nameof(param));
        }

        var property = param.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return (int)(property?.GetValue(param) ?? throw new InvalidOperationException($"Missing {propertyName}"));
    }

    private static Guid ReadAnonymousGuid(object? param, string propertyName)
    {
        if (param == null)
        {
            throw new ArgumentNullException(nameof(param));
        }

        var property = param.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return (Guid)(property?.GetValue(param) ?? throw new InvalidOperationException($"Missing {propertyName}"));
    }

    private static string? ReadAnonymousString(object? param, string propertyName)
    {
        if (param == null)
        {
            throw new ArgumentNullException(nameof(param));
        }

        var property = param.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(param) as string;
    }

    private static TValue ReadParameter<TValue>(NpgsqlParameter[] parameters, string name)
    {
        var parameter = parameters.FirstOrDefault(item => item.ParameterName == name)
            ?? throw new InvalidOperationException($"Missing parameter {name}");

        if (parameter.Value == null || parameter.Value == DBNull.Value)
        {
            return default!;
        }

        if (parameter.Value is TValue typedValue)
        {
            return typedValue;
        }

        if (parameter.Value is Array array && typeof(TValue).IsArray)
        {
            var elementType = typeof(TValue).GetElementType()!;
            var converted = Array.CreateInstance(elementType, array.Length);
            array.CopyTo(converted, 0);
            return (TValue)(object)converted;
        }

        return (TValue)Convert.ChangeType(parameter.Value, typeof(TValue));
    }
}
