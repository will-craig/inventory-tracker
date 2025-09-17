using MongoDB.Driver;

namespace Stockpile.Api.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            SeedDatabase();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
        
        void SeedDatabase()
        {
            var serviceProvider = app.Services;
            using var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            var db = scopedProvider.GetRequiredService<IMongoDatabase>();
            SeedData.Initialize(db);
        }
    }
}


