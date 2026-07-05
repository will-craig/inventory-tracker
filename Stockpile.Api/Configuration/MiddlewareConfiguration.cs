namespace Stockpile.Api.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        var swaggerEnabled = app.Configuration.GetValue("Swagger:Enabled", app.Environment.IsDevelopment());
        var swaggerJsonEnabled = app.Configuration.GetValue("Swagger:JsonEnabled", swaggerEnabled);
        var swaggerUiEnabled = app.Configuration.GetValue("Swagger:UiEnabled", swaggerEnabled);
        var isAgentLocal = app.Environment.IsEnvironment("AgentLocal");

        if (swaggerJsonEnabled)
        {
            app.UseSwagger();
        }

        if (swaggerUiEnabled)
        {
            app.UseSwaggerUI();
        }

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

