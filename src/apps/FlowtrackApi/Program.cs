using dotenv.net;
using FlowTrack.Iam;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
new DotEnvCharger().Load(["../../../.env"]);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.ProvideIam();
builder.Services.DiscoverServices(["FlowTrack*.dll"]);
builder.Services.AddKeyedScoped<Context>(
    "IAM",
    (sp, _) =>
    {
        var dbContext = sp.GetRequiredService<IamDbContext>();
        var transaction = new EfCoreTransaction(dbContext);
        return new Context(transaction);
    }
);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "IamCookie";
        options.DefaultChallengeScheme = "IamCookie";
    })
    .AddScheme<AuthenticationSchemeOptions, IamAuthenticationHandler>("IamCookie", options => { });

builder
    .Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(
        new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes("IamCookie")
            .RequireAuthenticatedUser()
            .Build()
    );

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
        if (exception == null)
            return;

        if (exception is ActionHandlerExecutionException actionHandlerExecutionException)
        {
            var cause = actionHandlerExecutionException.GetCause();
            if (cause is DomainException ex)
            {
                var (statusCode, httpErrorException) = DomainToHttpExceptionMapper.Map(ex);
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(httpErrorException);
                return;
            }
        }

        if (exception is DomainException domainException)
        {
            var (statusCode, httpErrorException) = DomainToHttpExceptionMapper.Map(domainException);
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(httpErrorException);
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(
            new HttpErrorResponse("Internal Server Error", "exception.internal.server")
        );
    });
});

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
