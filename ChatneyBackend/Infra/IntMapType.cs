using System.Text.Json;
using HotChocolate.Features;
using HotChocolate.Language;
using HotChocolate.Text.Json;

namespace ChatneyBackend.Infra;

/// <summary>
/// Input-only scalar: a JSON object with integer keys and integer values, e.g. {"5": 0, "7": 1}.
/// Frontend codegen maps it to Record&lt;string, number&gt;.
/// </summary>
public class IntMapType : ScalarType<Dictionary<int, int>, ObjectValueNode>
{
    public IntMapType() : base("IntMap", BindingBehavior.Explicit)
    {
        Description = "A JSON object with integer keys and integer values, e.g. {\"5\": 0, \"7\": 1}.";
    }

    protected override Dictionary<int, int> OnCoerceInputLiteral(ObjectValueNode valueLiteral)
    {
        try
        {
            return valueLiteral.Fields.ToDictionary(
                field => int.Parse(field.Name.Value),
                field => ((IntValueNode)field.Value).ToInt32());
        }
        catch (Exception ex)
        {
            throw CreateCoerceInputLiteralError(valueLiteral, ex);
        }
    }

    protected override Dictionary<int, int> OnCoerceInputValue(JsonElement inputValue, IFeatureProvider context)
    {
        try
        {
            return inputValue.Deserialize<Dictionary<int, int>>()!;
        }
        catch (Exception ex)
        {
            throw CreateCoerceInputValueError(inputValue, ex);
        }
    }

    protected override void OnCoerceOutputValue(Dictionary<int, int> runtimeValue, ResultElement resultValue) =>
        throw CreateCoerceOutputValueError(runtimeValue);

    protected override ObjectValueNode OnValueToLiteral(Dictionary<int, int> runtimeValue) =>
        throw CreateValueToLiteralError(runtimeValue);
}
