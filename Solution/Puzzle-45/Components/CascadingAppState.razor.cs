using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace Puzzle_45.Components;

public partial class CascadingAppState : ComponentBase, IAppState
{
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState> authenticationStateTask { get; set; } = default!;

    private bool canAccessConfigPage = false;
    public bool CanAccessConfigPage
    {
        get => canAccessConfigPage;
        set
        {
            if (canAccessConfigPage != value)
            {
                canAccessConfigPage = value;
                // Save the state
                new Task(async () =>
                {
                    await Save();
                }).Start();
                // Force a re-render
                StateHasChanged();
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load the values when the component is first rendered
            await Load();
            StateHasChanged();
        }
    }

    public async Task Save()
    {
        var fileName = GetFileName();

        if (fileName != "")
        {
            try
            {
                // serialize the state to a JSON string
                var state = (IAppState)this;
                var json = JsonSerializer.Serialize(state);
                // write it to the file
                await File.WriteAllTextAsync(fileName, json);
            }
            catch (Exception ex)
            {

            }
        }
    }

    public async Task Load()
    {
        var fileName = GetFileName();
        if (fileName != "")
        {
            try
            {
                // read the JSON string from the file
                var data = await File.ReadAllTextAsync(fileName);
                // deserialize the JSON string to the state
                var state = JsonSerializer.Deserialize<AppState>(data);
                if (state != null)
                {
                    // set property values with reflection
                    var t = typeof(IAppState);
                    var props = t.GetProperties();
                    foreach (var prop in props)
                    {
                        object value = prop.GetValue(state);
                        prop.SetValue(this, value, null);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    /// <summary>
    /// Get a local filename from the logged-in user's email address
    /// </summary>
    /// <returns></returns>
    private string GetFileName()
    {
        string result = "";
        try
        {
            // Is this user authenticated?
            if (authenticationStateTask.Result.User.Identity?.IsAuthenticated ?? false)
            {
                // get the email address
                var email = authenticationStateTask.Result.User.Identity.Name;
                // convert dots and at symbols to underscores
                var key = email.Replace(".", "_").Replace("@", "_");
                // create the Data\TempAppState folder if it doesn't exist
                var folder = Path.Combine("Data", "TempAppState");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                // create the filename with the Data\TempAppState folder
                result = Path.Combine("Data", "TempAppState", $"{key}.json");
            }
        }
        catch (Exception ex)
        {
        }
        return result;
    }
}
