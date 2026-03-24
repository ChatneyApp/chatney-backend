using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using Npgsql;

namespace ChatneyBackend.Infra;

public interface IPgDatabaseItem<T>
{
    public T Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PgRepo<T, TI> where T : IPgDatabaseItem<TI>
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly string _tableName;
    private readonly string _idColumn;
    private static readonly PropertyInfo[] _classProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    static PgRepo()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public PgRepo(NpgsqlDataSource dataSource, string tableName, string idColumn = "id")
    {
        _dataSource = dataSource;
        _tableName = tableName;
        _idColumn = idColumn;
    }

    public async Task<T?> GetById(TI id)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(
            $"SELECT * FROM {_tableName} WHERE {_idColumn} = @Id limit 1",
            new { Id = id }
        );
    }

    public async Task<T?> GetOne(string whereClause, object? parameters = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(
            $"SELECT * FROM {_tableName} WHERE {whereClause} limit 1",
            parameters
        );
    }

    public async Task<List<T>> GetList(string? whereClause = null, object? parameters = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var sql = whereClause == null
            ? $"SELECT * FROM {_tableName}"
            : $"SELECT * FROM {_tableName} WHERE {whereClause}";

        var items = await connection.QueryAsync<T>(sql, parameters);
        return items.ToList();
    }

    public async Task InsertOne(T record)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        var properties = GetScaffoldableProperties()
            .Where(ShouldIncludeOnInsert)
            .ToArray();

        var columns = string.Join(", ", properties.Select(ToColumnName));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        await connection.ExecuteAsync(
            $"INSERT INTO {_tableName} ({columns}) values ({values})",
            ToParameters(record)
        );
    }

    public async Task InsertBulk(List<T> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        var properties = GetScaffoldableProperties()
            .Where(ShouldIncludeOnInsert)
            .ToArray();

        var columns = string.Join(", ", properties.Select(ToColumnName));
        var parameters = new DynamicParameters();
        var values = new List<string>(items.Count);

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var placeholders = new List<string>(properties.Length);

            foreach (var property in properties)
            {
                var parameterName = $"{property.Name}_{index}";
                placeholders.Add($"@{parameterName}");
                parameters.Add(parameterName, NormalizeParameterValue(property.GetValue(item)));
            }

            values.Add($"({string.Join(", ", placeholders)})");
        }

        await using var connection = await _dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(
            $"INSERT INTO {_tableName} ({columns}) VALUES {string.Join(", ", values)}",
            parameters
        );
    }

    public async Task<bool> DeleteById(object id) => await Delete($"{_idColumn} = @Id", new { Id = id }) > 0;

    public async Task<long> Delete(string whereClause, object? parameters = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.ExecuteAsync(
            $"DELETE FROM {_tableName} WHERE {whereClause}",
            parameters
        );
    }

    public async Task<bool> UpdateOne(T record)
    {
        SetUpdatedAt(record);

        var properties = GetScaffoldableProperties()
            .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var setClause = string.Join(", ", properties.Select(p => $"{ToColumnName(p)} = @{p.Name}"));
        var id = GetIdValue(record);

        await using var connection = await _dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync(
            $"UPDATE {_tableName} SET {setClause} WHERE {_idColumn} = @__id",
            MergeParameters(record, new Dictionary<string, object?> { ["__id"] = id })
        );

        return affected > 0;
    }

    private bool ShouldIncludeOnInsert(PropertyInfo property) =>
        !string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<PropertyInfo> GetScaffoldableProperties() =>
        _classProps.Where(p => p.CanRead && p.CanWrite);

    private static object ToParameters(T record)
    {
        var parameters = new DynamicParameters();

        foreach (var property in GetScaffoldableProperties())
        {
            parameters.Add(property.Name, NormalizeParameterValue(property.GetValue(record)));
        }

        return parameters;
    }

    private static object MergeParameters(T record, IDictionary<string, object?> extraParameters)
    {
        var parameters = new DynamicParameters(ToParameters(record));

        foreach (var pair in extraParameters)
        {
            parameters.Add(pair.Key, pair.Value);
        }

        return parameters;
    }

    private static object? NormalizeParameterValue(object? value)
    {
        if (value is HashSet<string> stringSet)
        {
            return stringSet.ToArray();
        }

        if (value is IEnumerable<string> stringEnumerable && value is not string)
        {
            return stringEnumerable.ToArray();
        }

        return value;
    }

    private static void SetUpdatedAt(T record)
    {
        var property = _classProps.First(p => p.Name == "UpdatedAt");
        if (property.CanWrite && property.PropertyType == typeof(DateTime))
        {
            property.SetValue(record, DateTime.UtcNow);
        }
    }

    private static object? GetIdValue(T record) =>
        _classProps.First(p => p.Name == "Id").GetValue(record);

    private static string ToColumnName(PropertyInfo property)
    {
        var attr = property.GetCustomAttribute<ColumnAttribute>();
        return attr?.Name ?? ToSnakeCase(property.Name);
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (char.IsUpper(current) && i > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString();
    }
}
