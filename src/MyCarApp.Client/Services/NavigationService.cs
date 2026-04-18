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
        // Remove leading slash for relative navigation
        var cleanPath = path.TrimStart('/');
        _nav.NavigateTo(_nav.BaseUri + cleanPath);
    }
   
    public string GetPath(string path)
    {
        var cleanPath = path.TrimStart('/');
        return _nav.BaseUri + cleanPath;
    }
}