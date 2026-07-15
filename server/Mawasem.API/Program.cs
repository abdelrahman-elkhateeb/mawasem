using Mawasem.API.Authorization;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Options;
using Mawasem.Application.Features.Customers.Interfaces;
using Mawasem.Application.Features.Employees.Interfaces;
using Mawasem.Application.Features.Roles.Interfaces;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Authentication;
using Mawasem.Infrastructure.Customers;
using Mawasem.Infrastructure.Employees;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Persistence.Seed;
using Mawasem.Infrastructure.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1" ,
        new OpenApiInfo
        {
            Title = "Mawasem API" ,
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer" ,
        new OpenApiSecurityScheme
        {
            Name = "Authorization" ,
            Type = SecuritySchemeType.Http ,
            Scheme = "bearer" ,
            BearerFormat = "JWT" ,
            In = ParameterLocation.Header ,

            Description =
                "Enter the JWT access token only. " +
                "Do not include the word Bearer."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "The DefaultConnection connection string is missing.");

builder.Services.AddDbContext<MawasemDbContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    });

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(
        JwtSettings.SectionName));

builder.Services.Configure<SuperAdminSeedOptions>(
    builder.Configuration.GetSection(
        SuperAdminSeedOptions.SectionName));

builder.Services.Configure<CustomerPasswordResetOptions>(
    builder.Configuration.GetSection(
        CustomerPasswordResetOptions.SectionName));

var jwtSettings =
    builder.Configuration
        .GetSection(JwtSettings.SectionName)
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "The JWT configuration section is missing.");

if ( string.IsNullOrWhiteSpace(jwtSettings.Issuer) )
{
    throw new InvalidOperationException(
        "Jwt:Issuer is required.");
}

if ( string.IsNullOrWhiteSpace(jwtSettings.Audience) )
{
    throw new InvalidOperationException(
        "Jwt:Audience is required.");
}

if ( string.IsNullOrWhiteSpace(jwtSettings.Key) )
{
    throw new InvalidOperationException(
        "Jwt:Key is required. Store it using .NET User Secrets.");
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
        "Jwt:Key must be a valid Base64 value." ,
        exception);
}

if ( jwtKeyBytes.Length < 32 )
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 bytes.");
}

if ( jwtSettings.AccessTokenMinutes <= 0 )
{
    throw new InvalidOperationException(
        "Jwt:AccessTokenMinutes must be greater than zero.");
}

if ( jwtSettings.RefreshTokenDays <= 0 )
{
    throw new InvalidOperationException(
        "Jwt:RefreshTokenDays must be greater than zero.");
}

builder.Services
    .AddIdentityCore<ApplicationUser>(
        options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // Customer accounts do not currently require an email.
            // Dashboard email uniqueness is handled by the
            // dashboard authentication service.
            options.User.RequireUniqueEmail = false;

            options.Lockout.AllowedForNewUsers = true;

            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<MawasemDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true ,
                    ValidIssuer = jwtSettings.Issuer ,

                    ValidateAudience = true ,
                    ValidAudience = jwtSettings.Audience ,

                    ValidateIssuerSigningKey = true ,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            jwtKeyBytes) ,

                    ValidateLifetime = true ,
                    RequireExpirationTime = true ,

                    ClockSkew =
                        TimeSpan.FromSeconds(30) ,

                    NameClaimType =
                        ClaimTypes.Name ,

                    RoleClaimType =
                        ClaimTypes.Role
                };
        });

builder.Services.AddAuthorization(
    options =>
    {
        foreach ( var permission in SystemPermissions.All )
        {
            options.AddPolicy(
                permission ,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.AddRequirements(
                        new PermissionAuthorizationRequirement(
                            permission));
                });
        }
    });

builder.Services.AddScoped<
    IAuthorizationHandler ,
    PermissionAuthorizationHandler>();

builder.Services.AddSingleton(
    TimeProvider.System);

builder.Services.AddScoped<
    ITokenService ,
    JwtTokenService>();

builder.Services.AddScoped<
    ICustomerAuthenticationService ,
    CustomerAuthenticationService>();

builder.Services.AddScoped<
    ICustomerPasswordResetService ,
    CustomerPasswordResetService>();

if ( builder.Environment.IsDevelopment() )
{
    builder.Services.AddScoped<
        ICustomerPasswordResetCodeSender ,
        DevelopmentCustomerPasswordResetCodeSender>();
}

builder.Services.AddScoped<
    IDashboardAuthenticationService ,
    DashboardAuthenticationService>();

builder.Services.AddScoped<
    IDashboardUserProfileService ,
    DashboardUserProfileService>();

builder.Services.AddScoped<
    ICustomerManagementService ,
    CustomerManagementService>();

builder.Services.AddScoped<
    IEmployeeManagementService ,
    EmployeeManagementService>();

builder.Services.AddScoped<
    IRolePermissionManagementService ,
    RolePermissionManagementService>();

builder.Services.AddScoped<
    IdentityRoleSeeder>();

builder.Services.AddScoped<
    IdentityPermissionSeeder>();

builder.Services.AddScoped<
    FirstSuperAdminSeeder>();

var app = builder.Build();

await using ( var scope =
    app.Services.CreateAsyncScope() )
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

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();