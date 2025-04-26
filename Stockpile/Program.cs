using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Stockpile;
using Stockpile.Config;
using Stockpile.DAL.Repositories;
using Stockpile.Services;

var builder = WebApplication.CreateBuilder(args);
    
BuildConfiguration();
RegisterServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    SeedDatabase();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

void BuildConfiguration()
{
    builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("DatabaseConfig"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
}

void RegisterServices()
{
    builder.Services.AddSingleton<IMongoClient>(sp =>
    {
        var config = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
        return new MongoClient(config.ConnectionString);
    });

    builder.Services.AddScoped<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        var config = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
        return client.GetDatabase(config.DatabaseName);
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };
        });
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Fridge Tracker API", Version = "v1" });
        // Adding Authorization header to Swagger UI
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
                }, []
            }
        });
    });

    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<IUserProfileService, UserProfileService>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
    builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
}

void SeedDatabase()
{
    var serviceProvider = app.Services;
    using var scope = serviceProvider.CreateScope();
    var scopedProvider = scope.ServiceProvider;
    var db = scopedProvider.GetRequiredService<IMongoDatabase>();

    // Run seeding or other startup tasks
    SeedData.Initialize(db);
}