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
        var cleanPath = path.TrimStart('/');
        _nav.NavigateTo(cleanPath, forceLoad: false);
    }

    public string GetPath(string path)
    {
        var cleanPath = path.TrimStart('/');
        return _nav.BaseUri + cleanPath;
    }
}