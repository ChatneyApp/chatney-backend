using HotChocolate;

namespace ChatneyBackend.Infra;

public class ErrorCodes
{
    public const string ForbiddenAction = "FORBIDDEN_ACTION";
    public const string NotFound = "NOT_FOUND";
    public const string InvalidNickname = "INVALID_NICKNAME";
    public const string NicknameTaken = "NICKNAME_TAKEN";
    public const string RoleNotFound = "ROLE_NOT_FOUND";

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

    public static void ThrowInvalidNickname()
    {
        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(InvalidNickname)
                .SetCode(InvalidNickname)
                .Build());
    }

    public static void ThrowNicknameTaken()
    {
        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(NicknameTaken)
                .SetCode(NicknameTaken)
                .Build());
    }

    public static void ThrowRoleNotFound()
    {
        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(RoleNotFound)
                .SetCode(RoleNotFound)
                .Build());
    }
}
