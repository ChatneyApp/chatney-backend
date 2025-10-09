using System.Security.Claims;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Utils;
using MongoDB.Driver;

namespace ChatneyBackend.Infra.Middleware;

public static class HttpContextExtensions
{
    public static User GetCurrentUser(this HttpContext ctx)
    {
        return ctx.Items["CurrentUser"] as User;
    }

}

public static class ClaimsPincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.Claims.First(claim => claim.Type == ClaimTypes.Sid).Value;
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;
    }
}

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppConfig config, IMongoDatabase mongo)
    {
        try
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                var identityValid = JwtHelpers.ValidateJwtToken(token, config.JwtSecret);

                if (identityValid != null)
                {
                    var email = identityValid.Claims.First(c => c.Type == ClaimTypes.Email);
                    var sub = identityValid.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);

                    if (email != null && sub != null)
                    {
                        var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Sid, sub.Value),
                            new Claim(ClaimTypes.Email, email.Value)
                        }, "CustomAuth");

                        context.User = new ClaimsPrincipal(identity);
                        var user = (await mongo.GetCollection<User>(DomainSettings.UserCollectionName).FindAsync(u => u.Id == sub.Value)).ToList();
                        context.Items["CurrentUser"] = user[0];
                    }
                }
            }

            await _next(context);
        }
        catch (Exception error)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($@"{{""error"":500,""reason"":""{error.Message}""}}");
        }
    }
}
