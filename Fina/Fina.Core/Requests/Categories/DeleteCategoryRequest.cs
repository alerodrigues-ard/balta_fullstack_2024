using System.ComponentModel.DataAnnotations;

namespace Fina.Core.Requests.Categories;

public class DeleteCategoryRequest : Request
{
    [Required(ErrorMessage = "O Id da categoria é obrigatório")]
    public long Id { get; set; }
}
