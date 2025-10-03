using System;

namespace YourGame.UI.Widgets.Services
{
    public interface IThemeService : IDisposable
    {
        event Action ThemeChanged;
        UIStyle GetStyle(string styleKey);
    }
}
