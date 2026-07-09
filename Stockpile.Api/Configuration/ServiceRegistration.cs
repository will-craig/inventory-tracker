using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
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
        
        if (builder.Environment.EnvironmentName.Equals("AgentLocal", StringComparison.OrdinalIgnoreCase))
            configuration.AddUserSecrets<Program>(optional: true);
        
        services.Configure<DatabaseConfig>(configuration.GetSection("DatabaseConfig"));
        services.Configure<DatabaseInitializationConfig>(configuration.GetSection(DatabaseInitializationConfig.SectionName));
        services.Configure<InventoryAgentConfig>(configuration.GetSection(InventoryAgentConfig.SectionName));
        services.Configure<InventoryDigestConfig>(configuration.GetSection(InventoryDigestConfig.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            return new MongoClient(config.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var config = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            return client.GetDatabase(config.DatabaseName);
        });

        services.AddControllers() 
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); 
        
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();

        var azureAdEnabled = configuration.GetValue("AzureAd:Enabled", true);
        if (azureAdEnabled)
        {
            // Use Microsoft Identity Web for Entra ID authentication
            services.AddAuthentication("Bearer")
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, InventoryAgentAuthenticationHandler>(
                    InventoryAgentConfig.AuthenticationScheme, _ => { });
        }
        else
        {
            services.AddAuthentication(InventoryAgentConfig.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, InventoryAgentAuthenticationHandler>(
                    InventoryAgentConfig.AuthenticationScheme, _ => { });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy(InventoryAgentConfig.PolicyName, policy =>
            {
                policy.AuthenticationSchemes.Add(InventoryAgentConfig.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Fridge Tracker API", Version = "v1" });
            c.EnableAnnotations();
            c.DocumentFilter<InventoryAgentSecurityOperationFilter>();
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);

            if (azureAdEnabled)
            {
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
            }

            c.AddSecurityDefinition(InventoryAgentConfig.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = InventoryAgentConfig.HeaderName,
                Type = SecuritySchemeType.ApiKey,
                Scheme = InventoryAgentConfig.AuthenticationScheme,
                Description = "Inventory agent API key. Send the raw API key in the X-Inventory-Agent-Key header."
            });
        });
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowDevelopment", policy =>
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());

            options.AddPolicy("AllowConfiguredOrigins", policy =>
            {
                var origins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        return builder;
    }
}
