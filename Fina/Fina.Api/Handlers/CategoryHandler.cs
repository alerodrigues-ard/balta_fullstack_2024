using Fina.Api.Data;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class CategoryHandler(AppDbContext context) : ICategoryHandler
{
    public async Task<Response<Category?>> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description
        };

        try
        {
            // No trecho abaixo, para fazer rollback das alterações, basta não 
            // chamar SaveChengesAsync
            await context.Categories.AddAsync(category);    // Em memória
            await context.SaveChangesAsync();   // Efetiva alterações na base

            return new Response<Category?>(category, 201, "Categoria criada com sucesso");
        }
        catch (Exception)
        {
            // *** Nunca é recomendado simplesmente silenciar a exceção
            // Para logar erros pode usar Serilog, OpenTelem, entre outros

            return new Response<Category?>(null, 500, "Não foi possível criar a categoria");
        }
    }

    public async Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request)
    {
        try
        {
            // Reidratação (buscar dados do banco de dados)
            var category = await context
                .Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            // No trecho abaixo, para fazer rollback das alterações, basta não 
            // chamar SaveChengesAsync
            context.Categories.Remove(category);    // Em memória
            await context.SaveChangesAsync();   // Efetiva alterações na base

            return new Response<Category?>(category, message: "Categoria excluída com sucesso");
        }
        catch (Exception)
        {
            return new Response<Category?>(null, 500, "Não foi possível excluir a categoria");
        }
    }

    public async Task<PagedResponse<List<Category>?>> GetAllAsync(GetAllCategoriesRequest request)
    {
        try
        {
            // O trecho abaixo não faz cache dos dados

            // A query abaixo não está materializada, ou neja, ela é criada com o
            // comando SELECT, mas não recebe dados da base. Ela é do tipo 
            // IQueryable.
            var query = context
                .Categories
                .AsNoTracking()
                .Where(x => x.UserId == request.UserId)
                .OrderBy(x => x.Title);

            // Neste momento ocorre a materialização dos dados da query, ao
            // retornar uma List<Category> em catecories
            var categories = await query
                .Skip((request.PageNumber - 1) * request.PageSize)  // Paginação - Quantos registros eu quero pular antes de pegar registros
                .Take(request.PageSize)                             // Paginação - Quantos registros eu quero pegar da base depois do salto
                .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Category>?>(
                categories,
                count,
                request.PageNumber,
                request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Category>?>(null, 500, "Não foi possível recuperar as categorias");
        }
    }

    public async Task<Response<Category?>> GetByIdAsync(GetCategoryByIdRequest request)
    {
        try
        {
            // Reidratação (buscar dados do banco de dados)
            // Usar AsNoTracking quando quiser apenas buscar dados para 
            // exibição (é mais otimizada)
            var category = await context
                .Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return category is null
                ? new Response<Category?>(null, 404, "Categoria não encontrada")
                : new Response<Category?>(category);
        }
        catch (Exception)
        {
            return new Response<Category?>(null, 500, "Não foi possível recuperar a categoria");
        }
    }

    public async Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request)
    {
        try
        {
            // Reidratação (buscar dados do banco de dados)
            var category = await context
                .Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            category.Title = request.Title;
            category.Description = request.Description;

            // No trecho abaixo, para fazer rollback das alterações, basta não 
            // chamar SaveChengesAsync
            context.Categories.Update(category);    // Em memória
            await context.SaveChangesAsync();   // Efetiva alterações na base

            return new Response<Category?>(category, message: "Categoria criada com sucesso");
        }
        catch (Exception)
        {
            // *** Nunca é recomendado simplesmente silenciar a exceção
            // Para logar erros pode usar Serilog, OpenTelem, entre outros

            return new Response<Category?>(null, 500, "Não foi possível atualizar a categoria");
        }
    }
}
