using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Stockpile.Api.Configuration.Models;
using Stockpile.Api.Services;
using Stockpile.DAL.Repositories;

namespace Stockpile.Api.Configuration;

public static class ServiceRegistration
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.Configure<DatabaseConfig>(configuration.GetSection("DatabaseConfig"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            return new MongoClient(config.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var config = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            Console.WriteLine($"Database Name: {config.DatabaseName}");
            return client.GetDatabase(config.DatabaseName);
        });

        services.AddControllers() 
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); 
        
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
                var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Fridge Tracker API", Version = "v1" });
            c.EnableAnnotations();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Please enter your token in the format: Bearer {your_token}"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }, new string[]{}
                }
            });
        });
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "AllowAny",
                policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // dev only
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
        
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        return builder;
    }
}

