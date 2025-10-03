using UnityEngine;
using YourGame.UI.Widgets;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public class UITransition : MonoBehaviour
{
    private UIWidget _widgetController;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    private Vector3 _originalScale;
    private Vector2 _originalPosition;

    void Awake()
    {
        _widgetController = GetComponent<UIWidget>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
        _originalPosition = _rectTransform.anchoredPosition;
    }

    public void Show()
    {
        if (_widgetController.IsVisible && gameObject.activeSelf) return;

        gameObject.SetActive(true);
        if (_widgetController.OnShowStart != null) _widgetController.OnShowStart.Invoke();
        
        var style = _widgetController.appliedStyle;
        if (style == null) return;

        if ((style.transition & UIWidget.VisualTransition.Fade) != 0) _canvasGroup.alpha = 0f;
        if ((style.transition & UIWidget.VisualTransition.Slide) != 0) _rectTransform.anchoredPosition = _originalPosition + style.slideTransition.startOffset;
        if ((style.transition & UIWidget.VisualTransition.Scale) != 0) _rectTransform.localScale = Vector3.zero;
        
        if ((style.transition & UIWidget.VisualTransition.Fade) != 0)
            UITweener.Instance.AddTween(f => _canvasGroup.alpha = f, _canvasGroup.alpha, 1f, style.animationDuration, style.animationEaseType);

        if ((style.transition & UIWidget.VisualTransition.Slide) != 0)
            UITweener.Instance.AddTween(v => _rectTransform.anchoredPosition = v, _rectTransform.anchoredPosition, _originalPosition + style.slideTransition.endOffset, style.animationDuration, style.animationEaseType);

        if ((style.transition & UIWidget.VisualTransition.Scale) != 0)
            UITweener.Instance.AddTween(v => _rectTransform.localScale = v, _rectTransform.localScale, _originalScale, style.animationDuration, style.animationEaseType, () => {
                if (_widgetController.OnShowComplete != null) _widgetController.OnShowComplete.Invoke();
            });
        else
            UITweener.Instance.AddDelay(style.animationDuration, () => { if (_widgetController.OnShowComplete != null) _widgetController.OnShowComplete.Invoke(); });
    }

    public void Hide()
    {
        if (!_widgetController.IsVisible && !gameObject.activeSelf) return;

        if (_widgetController.OnHideStart != null) _widgetController.OnHideStart.Invoke();
        
        var style = _widgetController.appliedStyle;
        if (style == null) return;
        
        System.Action onHideComplete = () => {
            if (_widgetController.OnHideComplete != null) _widgetController.OnHideComplete.Invoke();
            if (_widgetController.deactivateOnHide)
            {
                gameObject.SetActive(false);
            }
        };

        if ((style.transition & UIWidget.VisualTransition.Fade) != 0)
            UITweener.Instance.AddTween(f => _canvasGroup.alpha = f, _canvasGroup.alpha, 0f, style.animationDuration, style.animationEaseType);

        if ((style.transition & UIWidget.VisualTransition.Slide) != 0)
            UITweener.Instance.AddTween(v => _rectTransform.anchoredPosition = v, _rectTransform.anchoredPosition, _originalPosition + style.slideTransition.startOffset, style.animationDuration, style.animationEaseType);

        if ((style.transition & UIWidget.VisualTransition.Scale) != 0)
            UITweener.Instance.AddTween(v => _rectTransform.localScale = v, _rectTransform.localScale, Vector3.zero, style.animationDuration, style.animationEaseType, onHideComplete);
        else
             UITweener.Instance.AddDelay(style.animationDuration, onHideComplete);
    }
}