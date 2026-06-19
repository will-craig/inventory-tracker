using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
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
        // Use Microsoft Identity Web for Entra ID authentication
        services.AddAuthentication("Bearer")
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
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
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document, null),
                    []
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
