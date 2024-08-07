# Blazor Puzzle #45

## Where'd my Navlink Go?

YouTube Video: https://youtu.be/EXvigihEhmk

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



