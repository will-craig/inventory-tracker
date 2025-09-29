using MongoDB.Driver;
using Stockpile.Api.Configuration;

var app = WebApplication
    .CreateBuilder(args)
    .RegisterServices()
    .Build()
    .ConfigureMiddleware();

SeedDatabase();  
app.Run();
void SeedDatabase()
{
    var serviceProvider = app.Services;
    using var scope = serviceProvider.CreateScope();
    var scopedProvider = scope.ServiceProvider;
    var db = scopedProvider.GetRequiredService<IMongoDatabase>();
    SeedData.Initialize(db);
}  