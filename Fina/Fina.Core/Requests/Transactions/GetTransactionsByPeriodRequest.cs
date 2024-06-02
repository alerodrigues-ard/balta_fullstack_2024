namespace Fina.Core.Requests.Transactions;
public class GetTransactionsByPeriodRequest : PagedRequest
{
    public DateTime? StartDate { get; set; }    // O default será o primeiro dia do mês
    public DateTime? EndDate { get; set; }      // O default será o último dia do mês
}
