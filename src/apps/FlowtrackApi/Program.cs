using dotenv.net;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Iam.Services;
using FlowTrack.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

DotEnvOptions options = new(envFilePaths: ["../../../.env"]);
DotEnv.Load(options);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<AuthCookieSetter>();

builder.Services.ProvideShared();
builder.Services.ProvideIam();

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
