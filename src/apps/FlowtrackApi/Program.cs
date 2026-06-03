using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared;
using FlowTrack.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.ProvideShared();
builder.Services.ProvideIam();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast");

app.MapPost(
    "/auth/signin",
    async (HttpContext context) =>
    {
        var queryBus = context.RequestServices.GetRequiredService<IQueryBus>();
        var email = context.Request.Form["email"].ToString();
        var password = context.Request.Form["password"].ToString();
        var query = new SigninQry(Email: email, Password: password);

        var signinSuccess = await queryBus.Ask<SigninQry, SigninSuccess>(query);
        if (signinSuccess is not null)
        {
            context.Response.Cookies.Append(
                "ACCESS_TOKEN",
                signinSuccess.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1),
                }
            );

            context.Response.Cookies.Append(
                "REFRESH_TOKEN",
                signinSuccess.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                }
            );

            context.Response.StatusCode = StatusCodes.Status200OK;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
);
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
