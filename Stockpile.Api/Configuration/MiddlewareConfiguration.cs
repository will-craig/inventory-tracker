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
            app.UseCors("AllowAny");
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseCors("AllowAny");  //TODO: setup production CORS policy later
        }
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}


