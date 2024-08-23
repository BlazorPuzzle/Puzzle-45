# Blazor Puzzle #45

## No RCL Protection!

YouTube Video: https://youtu.be/

Blazor Puzzle Home Page: https://blazorpuzzle.com

### The Challenge:

This is a .NET 8 Global Server-Side Blazor Web App using Individual Accounts for ASP.NET Core Identity Auth.

We added a config page where the user can configure things (in the real world).

We have created a CascadingAppState component that is accessible as a Cascading Parameter.

It has a CanAccessConfigPage variable that we set to true in the MainLayout.razor OnAfterRenderAsync method on first render.

In the real world, the CanAccessConfigPage variable would be set based on a database (or service) query.

In the NavMenu, we only show the link to the config page if AppState.CanAccessConfigPage is true.

HOWEVER, if we navigate to the Account page (your email address), the NavLink does NOT show up in the NavMenu.

How can we fix this? HINT: The Account pages do not have interactivity, and therefore no state.

### The Solution:

The solution is to persist the AppState data on the server when it is mutated, and load it on demand. 

It has to be saved on the server, because the Account pages don't have interactivity. Therefore there is no JavaScript, or any client-side code at all. 

Whether we save it in a database, local JSON files, or some other server-side cache doesn't really matter. 

For our solution, we are going to save the serialized AppState to local JSON files based on the user's email address.

First we need to create an interface containing all the properties in CascadingAppState component:

*IAppState.cs*:

```c#
public interface IAppState
{
    bool CanAccessConfigPage { get; set; }
}
```

Next, we need a concrete implementation of this interface:

```c#
public class AppState : IAppState
{
    public bool CanAccessConfigPage { get; set; }
}
```

Now, let's replace *CascadingAppState.razor.cs* with code to support loading and saving:

```c#
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
```

The `GetFileName()` method returns a local file name based on the logged in email address, and creates the folder if it doesn't exist.

The `Load()` and `Save()` methods do exactly what you think they do.

Now, we have to access AppState in the Account pages.

Modify the *Components/Account/Shared/AccountLayout.razor* file to access and load `CascadingAppState`:

```c#
@inherits LayoutComponentBase
@layout Puzzle_45.Components.Layout.MainLayout
@inject NavigationManager NavigationManager

@if (HttpContext is null)
{
    <p>Loading...</p>
}
else
{
    @Body
}

@code {
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    [CascadingParameter] 
    private CascadingAppState? AppState { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (HttpContext is null)
        {
            // If this code runs, we're currently rendering in interactive mode, so there is no HttpContext.
            // The identity pages need to set cookies, so they require an HttpContext. To achieve this we
            // must transition back from interactive mode to a server-rendered page.
            NavigationManager.Refresh(forceReload: true);
        }
        await AppState.Load();
    }
}
```

Here's how it works.

The `MainLayout` component runs in interactive mode. This is where we are setting the AppState.CanAccessConfigPage property to true on first render. 

When we do that, the serialized AppState is saved.

When we click on the account page (your email address) in the `NavMenu`, interactivity goes away. The *AccountLayout.razor* layout takes over. When it loads, the data is loaded from the JSON file.

The `NavMenu` sees that the `CanAccessConfigPage` property is true, and therefore shows the `NavLink` for the config page.

Boom!
