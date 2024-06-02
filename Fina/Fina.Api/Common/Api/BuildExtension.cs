using Fina.Api.Data;
using Fina.Api.Handlers;
using Fina.Core;
using Fina.Core.Handlers;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Common.Api;

public static class BuildExtension
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        ApiConfiguration.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnectionString") ?? string.Empty;
        Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;
        Configuration.FrontendUrl = builder.Configuration.GetValue<string>("FrontendUrl") ?? string.Empty;
    }

    // Adiciona serviços de documentação para a API usando o Swagger
    public static void AddDocumentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(x =>
        {
            // Garante que o Swagger usará os namespaces completos
            // No lugar de "Category", ele usará "Fina.Core.Models.Category"
            // Ajuda quando há classes com mesmo nome em namespaces diferentes
            x.CustomSchemaIds(n => n.FullName);
        });
    }

    // Adiciona serviços de banco de dados
    public static void AddDataContexts(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDbContext<AppDbContext>(
                options => 
                { 
                    options.UseSqlServer(ApiConfiguration.ConnectionString); 
                });
    }

    // CORS -> Cross Oriins Resource Sharing
    //      -> Compartilhamento de recursos entre domínios diferentes
    // Esta aplicação tem dois domínios/portas diferentes e precisa do CORS
    public static void AddCrossOrigin(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(
            options => options.AddPolicy(
                        ApiConfiguration.CorsPolicyName,
                        policy =>
                            policy.WithOrigins([
                                Configuration.FrontendUrl,
                                Configuration.BackendUrl
                                ])
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials()
                        )
                );
     }

     public static void AddServices(this WebApplicationBuilder builder)
     {
        builder.Services
            .AddTransient<ICategoryHandler, CategoryHandler>();

        builder.Services
            .AddTransient<ITransactionHandler, TransactionHandler>();
    }

}

