using Microsoft.AspNetCore.Components;

namespace MyCarApp.Client.Services;

public class NavigationService
{
    private readonly NavigationManager _nav;

    public NavigationService(NavigationManager nav)
    {
        _nav = nav;
    }

    public void NavigateTo(string path)
    {
        // Use NavigationManager directly with relative URI
        _nav.NavigateTo(path);
    }

    public string GetPath(string path)
    {
        return path;
    }
}