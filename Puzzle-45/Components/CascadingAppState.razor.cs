using Microsoft.AspNetCore.Components;

namespace Puzzle_45.Components;

public partial class CascadingAppState : ComponentBase
{

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    private bool canAccessConfigPage = false;
    public bool CanAccessConfigPage
    {
        get => canAccessConfigPage;
        set
        {
            if (canAccessConfigPage != value)
            {
                canAccessConfigPage = value;
                //// Force a re-render
                StateHasChanged();
            }
        }
    }
}
