using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using NetSentinel.Api.Services;
using NetSentinel.Api.Models;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// --- AJUSTE NO CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndRelease", policy =>
    {
        policy.WithOrigins(
                "http://localhost",        // Porta 80 do Docker Web
                "http://127.0.0.1",
                "http://192.168.5.80",
                "http://192.168.110.65",
                "http://localhost:3000",   // Dev React
                "http://localhost:5173"    // Dev Vite/Next
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi(); // Gera o JSON em /openapi/v1.json


builder.Services.AddHttpClient<OllamaVulnerabilityService>(client => 
{
    var url = builder.Configuration["Ollama:BaseUrl"] ?? "http://host.docker.internal:11434";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHostedService<NetSentinel.Api.Workers.VulnerabilityScannerWorker>();

builder.Services.AddHostedService<NetSentinel.Api.Workers.DeviceStatusWorker>();


var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".ps1"] = "text/plain";

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
app.UseCors("FrontEndRelease");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("[SENTINELA] Banco de dados migrado com sucesso");
                
    }catch (Exception ex)
    {
        logger.LogError(ex, "[SENTINELA - ERRO CRÍTICO] Erro ao aplicar migrações ou inicializar o banco de dados");
    }
}

app.Run(); 