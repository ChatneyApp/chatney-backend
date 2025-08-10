using System.Reflection;

namespace ChatneyBackend.Utils;

public static class PermissionsUtils
{
    public static string[] GetAllPermissions<T>()
    {
        var values = typeof(T)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .Select(f => f.GetRawConstantValue())
                    .Cast<string>()
                    .ToArray();

        return values;
    }
}
