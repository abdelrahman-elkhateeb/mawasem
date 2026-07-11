using Mawasem.API.Configurations;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

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
        // Password requirements
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Lock an account temporarily after repeated failed logins
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        // OTP is not required during normal login
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Customer email can remain optional.
        // Admin email uniqueness will be enforced in admin logic.
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<MawasemDbContext>()
    .AddDefaultTokenProviders();

// Identity password-reset tokens expire after 15 minutes
builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options =>
    {
        options.TokenLifespan = TimeSpan.FromMinutes(15);
    });

// ============================================================
// JWT settings
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

if ( string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32 )
{
    throw new InvalidOperationException(
        "JWT signing key is missing or too short.");
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
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key)) ,

                ClockSkew = TimeSpan.FromSeconds(30) ,

                NameClaimType = ClaimTypes.NameIdentifier ,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

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

if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();