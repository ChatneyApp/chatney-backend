using System.Security.Claims;

namespace ChatneyBackend.Infra.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("MIddleware hit");
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString().Replace("Bearer ", "");

            if (token == "secret")
            {
                Console.WriteLine("sdfsdfsdfds");
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, "admin")
                }, "CustomAuth");

                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
