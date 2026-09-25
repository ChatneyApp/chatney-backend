using System.Linq.Expressions;

namespace ChatneyBackend.Infra;

/// <summary>
/// C# 14 binds `array.Contains(x)` inside expression trees to MemoryExtensions.Contains(ReadOnlySpan).
/// RepoDb can't evaluate spans, so rewrite such calls back to Enumerable.Contains.
/// </summary>
public class SpanContainsRewriter : ExpressionVisitor
{
    private static readonly SpanContainsRewriter Instance = new();

    public static Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> expression) =>
        (Expression<Func<T, bool>>)Instance.Visit(expression);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(MemoryExtensions)
            && node.Method.Name == nameof(MemoryExtensions.Contains)
            && node.Arguments.Count == 2
            && node.Arguments[0] is MethodCallExpression { Method.Name: "op_Implicit" } spanConversion)
        {
            var source = Visit(spanConversion.Arguments[0]);
            var value = Visit(node.Arguments[1]);
            var elementType = value.Type;

            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [elementType], source, value);
        }

        return base.VisitMethodCall(node);
    }
}
