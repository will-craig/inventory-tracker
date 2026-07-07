namespace Stockpile.Api.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        var isAgentLocal = app.Environment.IsEnvironment("AgentLocal");

        app.UseSwagger();
        app.UseSwaggerUI();

        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowDevelopment");
        }
        else if (isAgentLocal)
        {
            app.UseCors("AllowConfiguredOrigins");
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseCors("AllowConfiguredOrigins");
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStockpileHealthChecks();
        app.MapControllers();
        return app;
    }
}
