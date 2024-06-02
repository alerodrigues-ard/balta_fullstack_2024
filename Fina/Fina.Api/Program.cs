using Fina.Api;
using Fina.Api.Common.Api;
using Fina.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
// A adição dos serviços deve seguir a ordem abaixo
builder.AddConfiguration();
builder.AddDataContexts();
builder.AddCrossOrigin();
builder.AddDocumentation();
builder.AddServices();

var app = builder.Build();
// Documentação da API (Swagger) apenas em desenvolvimento
if (app.Environment.IsDevelopment())
    app.ConfigureDevEnvironment();
app.UseCors(ApiConfiguration.CorsPolicyName);
app.MapEndpoints();

app.Run();

