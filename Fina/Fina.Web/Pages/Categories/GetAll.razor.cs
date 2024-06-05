using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fina.Web.Pages.Categories;

public partial class GetAllCategoriesPage : ComponentBase
{
	#region Properties

	public bool IsBusy { get; set; } = false;
	public List<Category> Categories { get; set; } = [];

	#endregion

	#region Services

	[Inject]
	public ICategoryHandler Handler { get; set; } = null!;

	[Inject]
	public ISnackbar Snackbar { get; set; } = null!;

	#endregion

	#region Overrides

	protected override async Task OnInitializedAsync()
	{
		IsBusy = true;
		try
		{
			var request = new GetAllCategoriesRequest();
			var result = await Handler.GetAllAsync(request);
			if (result.IsSuccess)
				Categories = result.Data ?? [];
		}
		catch (Exception ex)
		{
			Snackbar.Add(ex.Message, Severity.Error);
		}
		finally
		{
			IsBusy = false;
		}
	}


	#endregion

}
