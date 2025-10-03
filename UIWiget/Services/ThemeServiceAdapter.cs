using System;

namespace YourGame.UI.Widgets.Services
{
    public sealed class ThemeServiceAdapter : IThemeService
    {
        public event Action ThemeChanged;

        public ThemeServiceAdapter()
        {
            UIThemeManager.OnThemeChanged += HandleThemeChanged;
        }

        public UIStyle GetStyle(string styleKey)
        {
            if (string.IsNullOrEmpty(styleKey) || UIThemeManager.Instance == null)
            {
                return null;
            }

            return UIThemeManager.Instance.GetStyle(styleKey);
        }

        public void Dispose()
        {
            UIThemeManager.OnThemeChanged -= HandleThemeChanged;
        }

        private void HandleThemeChanged()
        {
            ThemeChanged?.Invoke();
        }
    }
}
