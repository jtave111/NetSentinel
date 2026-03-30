using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndRelease", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", // React
                "http://localhost:4200", // Angular
                "http://localhost:5173",   // Vite 
                "http://192.168.5.81:5173"
                  
              )
              .AllowAnyHeader()  // Permite mandar o Token JWT no cabeçalho
              .AllowAnyMethod(); // Permite POST, GET, PUT, DELETE
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
    
builder.Services.AddOpenApi();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".ps1"] = "text/plain";



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseCors("FrontEndRelease"); // libera o Front-end
app.UseAuthentication();         // JWT
app.UseAuthorization();          // Role

app.MapControllers();
app.Run();