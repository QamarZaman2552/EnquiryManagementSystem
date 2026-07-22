using Enquiry.api.Models;
using Enquiry.api.EmailService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/enquiry-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── Secrets ──
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("JWT_KEY environment variable is not set.");
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("dbcs")
    ?? throw new InvalidOperationException("DB_CONNECTION_STRING is not configured.");
var corsOrigin = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN")
    ?? builder.Configuration["Cors:AllowedOrigin"]
    ?? "http://localhost:4200";

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── Email ──
builder.Services.AddScoped<EmailService>();

// ── Database ──
builder.Services.AddDbContext<EnquiryDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// ── Health Checks ──
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EnquiryDbContext>();

// ── OpenTelemetry ──
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("EnquiryProAPI"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation());

// ── Rate Limiting ──
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.AddFixedWindowLimiter("Login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// ── CSRF Protection (check Origin header for state-changing methods) ──
app.Use(async (context, next) =>
{
    if (context.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE" &&
        !context.Request.Path.StartsWithSegments("/api/Auth/login") &&
        !context.Request.Path.StartsWithSegments("/api/Auth/refresh"))
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        var allowed = corsOrigin;

        if (string.IsNullOrEmpty(origin))
        {
            var referer = context.Request.Headers["Referer"].FirstOrDefault();
            if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refUri))
                origin = $"{refUri.Scheme}://{refUri.Host}:{refUri.Port}";
        }

        if (!string.IsNullOrEmpty(origin) && !string.Equals(origin, allowed, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "CSRF check failed" });
            return;
        }
    }
    await next();
});

// ── Security Headers ──
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' https://cdn.jsdelivr.net; img-src 'self' data: https://cdn.jsdelivr.net; connect-src 'self' https://cdn.jsdelivr.net;";
    await next();
});

app.UseSerilogRequestLogging();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            Log.Error(error.Error, "Unhandled exception");
            await context.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred. Please try again later.",
                detail = app.Environment.IsDevelopment() ? error.Error.Message : null
            });
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowFrontend");
app.UseRateLimiter();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
