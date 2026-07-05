using MongoDB.Driver;

namespace Stockpile.Api.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("AllowDevelopment");
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseCors("AllowConfiguredOrigins");
        }
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}


