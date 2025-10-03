using System;
using YourGame.UI;

namespace YourGame.UI.Widgets.Services
{
    public sealed class LocalizationServiceAdapter : ILocalizationService
    {
        public event Action LanguageChanged;

        private LanguageManager _cachedManager;

        public LocalizationServiceAdapter()
        {
            Subscribe();
        }

        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            Subscribe();

            if (_cachedManager != null)
            {
                return _cachedManager.GetString(key);
            }

            return LanguageManager.Instance != null ? LanguageManager.Instance.GetString(key) : string.Empty;
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (LanguageManager.Instance != null && LanguageManager.Instance != _cachedManager)
            {
                Unsubscribe();
                _cachedManager = LanguageManager.Instance;
                _cachedManager.OnLanguageChanged.AddListener(HandleLanguageChanged);
            }
        }

        private void Unsubscribe()
        {
            if (_cachedManager != null)
            {
                _cachedManager.OnLanguageChanged.RemoveListener(HandleLanguageChanged);
                _cachedManager = null;
            }
        }

        private void HandleLanguageChanged()
        {
            LanguageChanged?.Invoke();
        }
    }
}
