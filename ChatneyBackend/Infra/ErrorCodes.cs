using HotChocolate;

namespace ChatneyBackend.Infra;

public class ErrorCodes
{
    public const string ForbiddenAction = "FORBIDDEN_ACTION";
    public const string NotFound = "NOT_FOUND";

    public static void ThrowForbidden()
    {
        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(ForbiddenAction)
                .SetCode(ForbiddenAction)
                .Build());
    }

    public static void ThrowNotFound()
    {
        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(NotFound)
                .SetCode(NotFound)
                .Build());
    }
}
