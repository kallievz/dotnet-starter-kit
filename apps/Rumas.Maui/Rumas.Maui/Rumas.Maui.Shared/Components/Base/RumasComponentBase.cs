using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace Rumas.Maui.Shared.Components.Base;

public abstract class RumasComponentBase : ComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
}
