using Enquiry.api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Resolve secrets from environment variables (fallback to appsettings) ──
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                         ?? builder.Configuration.GetConnectionString("dbcs");
var corsOrigin = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN")
                 ?? builder.Configuration["Cors:AllowedOrigin"]
                 ?? "http://localhost:4200";

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT signing key is not configured. Set JWT_KEY environment variable or Jwt:Key in appsettings.");

if (string.IsNullOrWhiteSpace(dbConnectionString))
    throw new InvalidOperationException("Database connection string is not configured. Set DB_CONNECTION_STRING environment variable or ConnectionStrings:dbcs in appsettings.");

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(corsOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<EnquiryDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// ── JWT Authentication ──
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

var app = builder.Build();

// ── Global Exception Handler ──
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred. Please try again later.",
                detail = app.Environment.IsDevelopment() ? error.Error.Message : null
            });
        }
    });
});

// Run EF migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnquiryDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAngular");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
