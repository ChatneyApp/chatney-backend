using System.Diagnostics.Tracing;
using System.Security.Authentication;
using System.Security.Claims;
using ChatneyBackend.Utils;

namespace ChatneyBackend.Infra.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppConfig config)
    {
        try
        {
            Console.WriteLine("MIddleware hit");
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                var identityValid = JwtHelpers.ValidateJwtToken(token, config.JwtSecret);
                Console.WriteLine(identityValid.Identity.IsAuthenticated);

                if (identityValid != null)
                {
                    var email = identityValid.Claims.First(c => c.Type == ClaimTypes.Name);
                    var sub = identityValid.Claims.First(c => c.Type == "sub");

                    if (email != null && sub != null)
                    {
                        var identity = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.Sid, sub.Value),
                    new Claim(ClaimTypes.Email, email.Value)
                }, "CustomAuth");

                        context.User = new ClaimsPrincipal(identity);

                    }
                }
            }

            await _next(context);
        }
        catch (Exception error)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($@"{{""error"":401,""reason"":""Auth error""}}");
        }
    }
}
