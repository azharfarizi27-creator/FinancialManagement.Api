using System.Text;
using System.Threading.RateLimiting;
using FinancialManagement.Api.Data;
using FinancialManagement.Api.Middleware;
using FinancialManagement.Api.Repositories.Impl;
using FinancialManagement.Api.Repositories.Interfaces;
using FinancialManagement.Api.Services.Background;
using FinancialManagement.Api.Services.Impl;
using FinancialManagement.Api.Services.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

// QuestPDF License Configuration (Community License)
QuestPDF.Settings.License = LicenseType.Community;

// Enable Npgsql legacy timestamp behavior for seamless DateTime compatibility
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 2. CORS Configuration (Mendukung React Vite & Localhost manapun)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Mengizinkan localhost port berapapun
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Aman untuk auth berbasis cookie / token
    });
});

builder.Services.AddEndpointsApiExplorer();

// 3. Swagger Documentation with JWT Auth
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Masukkan JWT Token. Contoh: Bearer {token}"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// 4. Database Context (Supports both PostgreSQL & SQL Server)
var rawConnString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "";

string formattedConnString = rawConnString;
if (rawConnString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
    rawConnString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var uri = new Uri(rawConnString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? userInfo[0] : "";
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        formattedConnString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
    catch
    {
        formattedConnString = rawConnString;
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (formattedConnString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
        formattedConnString.Contains("Port=", StringComparison.OrdinalIgnoreCase) ||
        formattedConnString.Contains("Username=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(formattedConnString);
    }
    else
    {
        options.UseSqlServer(formattedConnString);
    }
});

// 5. Rate Limiting Policy for Auth & Sensitive Endpoints
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth-limit", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// 6. Dependency Injection: Repositories & Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IFinancialInsightRepository, FinancialInsightRepository>();
builder.Services.AddScoped<IFinancialInsightService, FinancialInsightService>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// File Storage & Export Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Background Worker: Automated Notification Scheduler
builder.Services.AddHostedService<NotificationSchedulerService>();

// 7. JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Mencegah error SSL saat local testing
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Waktu kedaluwarsa tepat sesuai setting
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-create database & tables on startup for PostgreSQL / Cloud DB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to execute Database.EnsureCreated()");
    }
}

// ==========================================
// PIPELINE & MIDDLEWARE (Urutan Sangat Krusial)
// ==========================================

// Enable Swagger in all environments for portfolio testing & live documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinancialManagement API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// 1. CORS ditaruh paling awal agar semua response (termasuk error) punya header CORS
app.UseCors("AllowAll");

// 2. Static Files (upload avatar/receipt)
app.UseStaticFiles();

// 3. Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// 4. Exception Handler (setelah CORS agar error 500 tetap terbaca di frontend)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 5. Rate Limiter
app.UseRateLimiter();

// 6. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Endpoint Mapping
app.MapControllers();

app.Run();
