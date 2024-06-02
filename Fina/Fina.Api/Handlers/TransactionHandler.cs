using System;
using Fina.Api.Data;
using Fina.Core.Common;
using Fina.Core.Enums;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Transactions;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class TransactionHandler(AppDbContext context) : ITransactionHandler
{
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 })
            request.Amount *= -1;

        var transaction = new Transaction
        {
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.Now,
            Amount = request.Amount,
            PaidOrReceivedAt = request.PaidOrReceivedAt,
            Title = request.Title,
            Type = request.Type
        };

        try
        {
            // No trecho abaixo, para fazer rollback das alterações, basta não 
            // chamar SaveChengesAsync
            await context.Transactions.AddAsync(transaction);    // Em memória
            await context.SaveChangesAsync();   // Efetiva alterações na base

            return new Response<Transaction?>(transaction, 201, "Transação criada com sucesso");
        }
        catch (Exception)
        {
            // *** Nunca é recomendado simplesmente silenciar a exceção
            // Para logar erros pode usar Serilog, OpenTelem, entre outros

            return new Response<Transaction?>(null, 500, "Não foi possível criar a transação");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            // Reidratação (buscar dados do banco de dados)
            var transaction = await context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encontrada");

            // No trecho abaixo, para fazer rollback das alterações, basta não 
            // chamar SaveChengesAsync
            context.Transactions.Remove(transaction);    // Em memória
            await context.SaveChangesAsync();   // Efetiva alterações na base

            return new Response<Transaction?>(transaction, message: "Transação excluída com sucesso");
        }
        catch (Exception)
        {
            return new Response<Transaction?>(null, 500, "Não foi possível excluir a transação");
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            // Reidratação (buscar dados do banco de dados)
            var transaction = await context
                .Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return transaction is null
                ? new Response<Transaction?>(null, 404, "Transação não encontrata")
                : new Response<Transaction?>(transaction);
        }
        catch (Exception)
        {
            return new Response<Transaction?>(null, 500, "Não foi possível recuperar a transação");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionsByPeriodRequest request)
    {
        try
        {
            request.StartDate ??= DateTime.Now.GetFirstDay();
            request.EndDate ??= DateTime.Now.GetLastDay();
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500, "Não foi possível determinar a data de início ou data de término do período");
        }

        try
        {
            // O trecho abaixo não faz cache dos dados

            // A query abaixo não está materializada, ou neja, ela é criada com o
            // comando SELECT, mas não recebe dados da base. Ela é do tipo 
            // IQueryable.
            var query = context
                .Transactions
                .AsNoTracking()
                .Where(x => 
                    x.UserId == request.UserId && 
                    x.PaidOrReceivedAt >= request.StartDate && 
                    x.PaidOrReceivedAt <= request.EndDate)
                .OrderBy(x => x.PaidOrReceivedAt);

            // Neste momento ocorre a materialização dos dados da query, ao
            // retornar uma List<Transaction> em catecories
            var transactions = await query
                .Skip((request.PageNumber - 1) * request.PageSize)  // Paginação - Quantos registros eu quero pular antes de pegar registros
                .Take(request.PageSize)                             // Paginação - Quantos registros eu quero pegar da base depois do salto
                .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Transaction>?>(
                transactions,
                count,
                request.PageNumber,
                request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500, "Não foi possível recuperar as transações");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 })
            request.Amount *= -1;

        try
        {
            // Reidratação (buscar dados do banco de dados)
            var transaction = await context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encontrada");

            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.Title = request.Title;
            transaction.Type = request.Type;
            transaction.PaidOrReceivedAt = request.PaidOrReceivedAt;

            // No trecho abaixo, para fazer rollback das alterações, basta não 
            // chamar SaveChengesAsync
            context.Transactions.Update(transaction);    // Em memória
            await context.SaveChangesAsync();   // Efetiva alterações na base

            return new Response<Transaction?>(transaction, message: "Transação atualizada com sucesso");
        }
        catch (Exception)
        {
            return new Response<Transaction?>(null, 500, "Não foi possível atualizar a transação");
        }
    }
}
