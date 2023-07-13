using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using APIContagem;
using APIContagem.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "APIContagem",
            Description = "Exemplo de implementação de Minimal API para Contagem de acessos e utilizando Rate Limiting", 
            Version = "v1",
            Contact = new OpenApiContact()
            {
                Name = "Renato Groffe",
                Url = new Uri("https://github.com/renatogroffe"),
            },
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("http://opensource.org/licenses/MIT"),
            }
        });
});

const string policyFixed = "fixed";
const string policyFixedQueueLimit = "fixed-queuelimit";
const string policyConcurrency = "concurrency";

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    limiterOptions.AddFixedWindowLimiter(policyName: policyFixed, options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromSeconds(5);
    });

    limiterOptions.AddFixedWindowLimiter(policyName: policyFixedQueueLimit, options =>
    {
        options.PermitLimit = 3;
        options.QueueLimit = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.Window = TimeSpan.FromSeconds(10);
    });

    limiterOptions.AddConcurrencyLimiter(policyName: policyConcurrency, options =>
    {
        options.PermitLimit = 5;
    });
});

builder.Services.AddSingleton<Contador>();

builder.Services.AddCors();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIContagem v1");
});

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseRateLimiter();

app.MapContador($"/contador/{policyFixed}")
    .RequireRateLimiting(policyFixed);

app.MapContador($"/contador/{policyFixedQueueLimit}")
    .RequireRateLimiting(policyFixedQueueLimit);

app.MapContador($"/contador/{policyConcurrency}")
    .RequireRateLimiting(policyConcurrency);

app.Run();