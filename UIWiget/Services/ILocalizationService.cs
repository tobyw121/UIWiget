using System;

namespace YourGame.UI.Widgets.Services
{
    public interface ILocalizationService : IDisposable
    {
        event Action LanguageChanged;
        string GetString(string key);
    }
}
