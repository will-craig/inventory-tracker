using Stockpile.Api.Configuration;

var app = WebApplication
    .CreateBuilder(args)
    .RegisterServices()
    .Build()
    .ConfigureMiddleware();
    
app.Run();