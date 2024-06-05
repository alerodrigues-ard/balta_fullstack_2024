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

	[Inject]
	public IDialogService Dialog { get; set; } = null!;

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

	#region Methods

	public async void OnDeleteButtonClickedAsync(long id, string title)
	{
		var result = await Dialog.ShowMessageBox(
			"ATENÇÃO",
			$"Ao prosseguir a categoria {title} será removida. Deseja continuar?",
			yesText: "Excluir",
			cancelText: "Cancelar");

		if (result is true)
			await OnDeleteAsync(id, title);

		// Instrui o Blazor a atualizar a tela, caso a categoria tenha sido excluída
		StateHasChanged();
	}

	public async Task OnDeleteAsync(long id, string title)
	{
		try
		{
			var request = new DeleteCategoryRequest
			{
				Id = id
			};
			await Handler.DeleteAsync(request);

			// Remove da tela a categoria que foi excluída
			Categories.RemoveAll(x => x.Id == id);

			Snackbar.Add($"Categoria {title} removida", Severity.Info);


		}
		catch (Exception ex)
		{
			Snackbar.Add(ex.Message, Severity.Error);
		}
	}

	#endregion

}
