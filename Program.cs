using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Duende.IdentityServer.Stores;
using UMS_BE.Data;
using UMS_BE.Repositories.Interfaces;
using UMS_BE.Repositories.Implementations;
using UMS_BE.Services.Interfaces;
using UMS_BE.Services.Implementations;
using UMS_BE.IdentityServer;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();

// Add Services
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add Duende IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
.AddDeveloperSigningCredential()
.AddInMemoryIdentityResources(new[]
{
    new Duende.IdentityServer.Models.IdentityResource
    {
        Name = "openid",
        DisplayName = "Your user identifier",
        Required = true,
        UserClaims = { "sub" }
    },
    new Duende.IdentityServer.Models.IdentityResource
    {
        Name = "profile",
        DisplayName = "User profile",
        Description = "Your user profile information",
        UserClaims = { "name", "given_name", "family_name", "email", "username" }
    },
    new Duende.IdentityServer.Models.IdentityResource
    {
        Name = "email",
        DisplayName = "Your email address",
        UserClaims = { "email" }
    },
    new Duende.IdentityServer.Models.IdentityResource
    {
        Name = "roles",
        DisplayName = "Your roles",
        UserClaims = { "role" }
    },
    new Duende.IdentityServer.Models.IdentityResource
    {
        Name = "permissions",
        DisplayName = "Your permissions",
        UserClaims = { "permission" }
    }
})
.AddInMemoryApiScopes(new[]
{
    new Duende.IdentityServer.Models.ApiScope("api", "Main API")
});

// Register custom IdentityServer services
builder.Services.AddTransient<IResourceOwnerPasswordValidator, CustomPasswordValidator>();
builder.Services.AddTransient<IProfileService, CustomProfileService>();
builder.Services.AddTransient<IClientStore, CustomClientStore>();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:7000";
    options.Audience = "api";
    options.RequireHttpsMetadata = false;
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "Admin");
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "UMS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseIdentityServer();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var passwordHashingService = services.GetRequiredService<IPasswordHashingService>();
        await DbInitializer.SeedAsync(context, passwordHashingService);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
