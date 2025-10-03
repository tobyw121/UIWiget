using YourGame.UI.Widgets.Services;

namespace YourGame.UI.Widgets.Tooltips
{
    public sealed class DefaultTooltipResolver : ITooltipResolver
    {
        private readonly IThemeService _themeService;
        private readonly ILocalizationService _localizationService;

        public DefaultTooltipResolver(IThemeService themeService, ILocalizationService localizationService)
        {
            _themeService = themeService;
            _localizationService = localizationService;
        }

        public string ResolveText(UIWidget widget)
        {
            if (widget == null || widget.Tooltip == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(widget.Tooltip.TooltipText))
            {
                return widget.Tooltip.TooltipText;
            }

            if (!string.IsNullOrEmpty(widget.styleKey))
            {
                var style = _themeService?.GetStyle(widget.styleKey);
                if (style != null && !string.IsNullOrEmpty(style.tooltipLocalizationKey))
                {
                    return _localizationService?.GetString(style.tooltipLocalizationKey) ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
