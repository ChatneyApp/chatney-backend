using HotChocolate;
using ErrorCodes = ChatneyBackend.Infra.ErrorCodes;

namespace ChatneyBackend.Tests.Infra;

public class ErrorCodesTests
{
    [Fact]
    public void ThrowForbidden_ThrowsGraphQlException()
    {
        var exception = Assert.Throws<GraphQLException>(ErrorCodes.ThrowForbidden);

        Assert.Equal(ErrorCodes.ForbiddenAction, exception.Message);
    }
}
