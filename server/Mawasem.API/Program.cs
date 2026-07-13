using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Options;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Authentication;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Database
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<MawasemDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================================
// ASP.NET Core Identity
// ============================================================

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<MawasemDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options =>
    {
        options.TokenLifespan = TimeSpan.FromMinutes(15);
    });

// ============================================================
// JWT configuration
// ============================================================

var jwtSection =
    builder.Configuration.GetSection(JwtSettings.SectionName);

var jwtSettings =
    jwtSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration was not found.");

if ( string.IsNullOrWhiteSpace(jwtSettings.Issuer) )
{
    throw new InvalidOperationException(
        "JWT issuer was not configured.");
}

if ( string.IsNullOrWhiteSpace(jwtSettings.Audience) )
{
    throw new InvalidOperationException(
        "JWT audience was not configured.");
}

if ( string.IsNullOrWhiteSpace(jwtSettings.Key) )
{
    throw new InvalidOperationException(
        "JWT signing key was not configured.");
}

byte[] jwtKeyBytes;

try
{
    jwtKeyBytes =
        Convert.FromBase64String(jwtSettings.Key);
}
catch ( FormatException exception )
{
    throw new InvalidOperationException(
        "JWT signing key must be a valid Base64 value." ,
        exception);
}

if ( jwtKeyBytes.Length < 32 )
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 bytes.");
}

if ( jwtSettings.AccessTokenMinutes <= 0 )
{
    throw new InvalidOperationException(
        "JWT access-token lifetime must be greater than zero.");
}

if ( jwtSettings.RefreshTokenDays <= 0 )
{
    throw new InvalidOperationException(
        "JWT refresh-token lifetime must be greater than zero.");
}

builder.Services.Configure<JwtSettings>(jwtSection);

// ============================================================
// First SuperAdmin seed configuration
// ============================================================

builder.Services.Configure<SuperAdminSeedOptions>(
    builder.Configuration.GetSection(
        SuperAdminSeedOptions.SectionName));

// ============================================================
// Authentication
// ============================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = false;
        options.RequireHttpsMetadata = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true ,
                ValidIssuer = jwtSettings.Issuer ,

                ValidateAudience = true ,
                ValidAudience = jwtSettings.Audience ,

                ValidateLifetime = true ,

                ValidateIssuerSigningKey = true ,
                IssuerSigningKey =
                    new SymmetricSecurityKey(jwtKeyBytes) ,

                ClockSkew = TimeSpan.FromSeconds(30) ,

                NameClaimType = ClaimTypes.NameIdentifier ,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

// JWT and refresh-token generation
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<
    ITokenService ,
    JwtTokenService>();

builder.Services.AddScoped<
    ICustomerAuthenticationService ,
    CustomerAuthenticationService>();

builder.Services.AddScoped<
    IDashboardAuthenticationService ,
    DashboardAuthenticationService>();

// ============================================================
// Identity seeders
// ============================================================

builder.Services.AddScoped<IdentityRoleSeeder>();
builder.Services.AddScoped<IdentityPermissionSeeder>();
builder.Services.AddScoped<FirstSuperAdminSeeder>();

// ============================================================
// Controllers
// ============================================================

builder.Services.AddControllers();

// ============================================================
// Swagger
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer" ,
        new OpenApiSecurityScheme
        {
            Name = "Authorization" ,
            Description =
                "Enter the JWT access token without writing the word Bearer." ,
            In = ParameterLocation.Header ,
            Type = SecuritySchemeType.Http ,
            Scheme = "bearer" ,
            BearerFormat = "JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// ============================================================
// Application pipeline
// ============================================================

var app = builder.Build();

await using ( var scope = app.Services.CreateAsyncScope() )
{
    var roleSeeder =
        scope.ServiceProvider
            .GetRequiredService<IdentityRoleSeeder>();

    await roleSeeder.SeedAsync();

    var permissionSeeder =
        scope.ServiceProvider
            .GetRequiredService<IdentityPermissionSeeder>();

    await permissionSeeder.SeedAsync();

    var firstSuperAdminSeeder =
        scope.ServiceProvider
            .GetRequiredService<FirstSuperAdminSeeder>();

    await firstSuperAdminSeeder.SeedAsync();
}

if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();