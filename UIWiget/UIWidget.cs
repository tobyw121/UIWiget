// Dateiname: UIWidget.cs (Final korrigiert für zuverlässige Textfarben)
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using YourGame.UI;

namespace YourGame.UI.Widgets
{
    public static class Easing
    {
        public enum EaseType { Linear, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseOutBack, EaseOutElastic }
        
        public static float GetValue(EaseType easeType, float start, float end, float value)
        {
            float d = end - start;
            switch (easeType)
            {
                case EaseType.EaseInQuad:
                    return d * value * value + start;
                case EaseType.EaseOutQuad:
                    return -d * value * (value - 2) + start;
                case EaseType.EaseInOutQuad:
                    value /= .5f;
                    if (value < 1) return d * 0.5f * value * value + start;
                    value--;
                    return -d * 0.5f * (value * (value - 2) - 1) + start;
                case EaseType.EaseOutBack:
                    float c1 = 1.70158f;
                    float c3 = c1 + 1f;
                    return d * (1 + c3 * Mathf.Pow(value - 1, 3) + c1 * Mathf.Pow(value - 1, 2)) + start;
                case EaseType.EaseOutElastic:
                    float c4 = (2 * Mathf.PI) / 3;
                    if (value == 0) return start;
                    if (value == 1) return end;
                    return d * (Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 10 - 0.75f) * c4) + 1) + start;
                default:
                    return Mathf.Lerp(start, end, value);
            }
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class UIWidget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum UIState { Interactive, Disabled, NotInteractive }

        [System.Flags]
        public enum VisualTransition
        {
            None = 0,
            ColorTint = 1,
            SpriteSwap = 2,
            Fade = 4,
            Scale = 8,
            Slide = 16
        }

        [Serializable]
        public class TooltipInfo
        {
            public bool Enabled = true;
            [TextArea] public string TooltipText = "";
            public float Delay = 0.5f;
            public Vector2 Offset = new Vector2(0, -30);
            public GameObject CustomTooltipPrefab;
            public object TooltipData;
        }

        [Serializable]
        public class ColorTintBlock
        {
            public Color normalColor = Color.white;
            public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            public Color pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        [Serializable]
        public class SlideTransition
        {
            public Vector2 startOffset = new Vector2(0, -50);
            public Vector2 endOffset = Vector2.zero;
        }

        [Tooltip("Eindeutiger Name des Widgets.")]
        [SerializeField] private string widgetName;
        
        [Header("Behavior")]
        [SerializeField] protected UIState startingState = UIState.Interactive;
        [SerializeField] protected bool deactivateOnHide = true;
        [SerializeField] protected bool focusOnShow = true;
        [SerializeField] protected KeyCode toggleKey = KeyCode.None;
        
        [Header("Animation")]
        [SerializeField] public float animationDuration = 0.25f;
        [SerializeField] public Easing.EaseType animationEaseType = Easing.EaseType.EaseOutQuad;
        
        [Header("Visuals")]
        [SerializeField] private VisualTransition transition = VisualTransition.Fade;
        [SerializeField] public Graphic targetGraphic;
        [SerializeField] private SlideTransition slideTransition = new SlideTransition();
        [Tooltip("Wackelt bei Mausberührung.")]
        [SerializeField] private bool wobbleOnHover = false;

        [Header("Theming")]
        [Tooltip("Der Style Key aus dem aktiven UIThemeData.")]
        [SerializeField] public string styleKey;
        
        [Header("Sound Events")]
        [SerializeField] protected AudioClip showSound;
        [SerializeField] protected AudioClip hideSound;
        [SerializeField] protected AudioClip clickSound;
        [SerializeField] protected AudioClip hoverSound;
        
        [Header("Tooltip")]
        [SerializeField] public TooltipInfo Tooltip;
        
        [Header("Navigation")]
        public UIWidget selectOnUp;
        public UIWidget selectOnDown;
        public UIWidget selectOnLeft;
        public UIWidget selectOnRight;
        
        [Header("Localization")]
        [Tooltip("Der Lokalisierungs-Schlüssel aus translations.json.")]
        [SerializeField]
        private string _localizationKey = "";
        
        private TMP_Text _textComponent;

        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                UpdateLocalizedText();
            }
        }

        public UnityEvent OnShowStart { get; set; } = new UnityEvent();
        public UnityEvent OnShowComplete { get; set; } = new UnityEvent();
        public UnityEvent OnHideStart { get; set; } = new UnityEvent();
        public UnityEvent OnHideComplete { get; set; } = new UnityEvent();
        public UnityEvent OnFocusGainedEvent { get; set; } = new UnityEvent();
        public UnityEvent OnFocusLostEvent { get; set; } = new UnityEvent();
        public UnityEvent<UIWidget, PointerEventData> OnClickEvent { get; set; } = new UnityEvent<UIWidget, PointerEventData>();
        public UnityEvent<UIWidget, bool> OnPressEvent { get; set; } = new UnityEvent<UIWidget, bool>();
        public UnityEvent<UIWidget, bool> OnHoverEvent { get; set; } = new UnityEvent<UIWidget, bool>();
        public UnityEvent<UIWidget, PointerEventData> OnDragStartEvent { get; set; } = new UnityEvent<UIWidget, PointerEventData>();
        public UnityEvent<UIWidget, PointerEventData> OnDragEvent { get; set; } = new UnityEvent<UIWidget, PointerEventData>();
        public UnityEvent<UIWidget, PointerEventData> OnDragEndEvent { get; set; } = new UnityEvent<UIWidget, PointerEventData>();
        public UnityEvent OnDragCancelled { get; set; } = new UnityEvent();
        public bool IsDraggable { get; protected set; } = false;
        public bool IsDropTarget { get; protected set; } = false;

        public string Name { get => widgetName; set => widgetName = value; }
        public bool IsVisible => _canvasGroup != null && gameObject.activeSelf && _canvasGroup.alpha > 0.99f;
        public UIState CurrentState { get; private set; }
        public object UserData { get; set; }
        public UIMenu ParentMenu { get; internal set; }
        public RectTransform RectTransform => _rectTransform;

        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;
        private Coroutine _animationCoroutine;
        private Coroutine _colorFadeCoroutine;
        private Coroutine _wobbleCoroutine;
        protected bool _isHovering;
        protected bool _isPressed;
        protected bool _isDragging;
        public Vector2 _originalPosition;
        private Vector3 _originalScale;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
            _textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
            _originalPosition = _rectTransform.anchoredPosition;
            _originalScale = _rectTransform.localScale;

            if (gameObject.activeInHierarchy) SetState(startingState);
            else
            {
                if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                SetState(UIState.Disabled);
                if (deactivateOnHide) gameObject.SetActive(false);
            }
        }

        protected virtual void OnEnable()
        {
            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.RegisterWidget(this);
            if (toggleKey != KeyCode.None && UIInputHandler.Instance != null)
                 UIInputHandler.Instance.RegisterToggleKey(toggleKey, this);
            
            UIThemeManager.OnThemeChanged += ApplyThemeStyle;
            if (LanguageManager.Instance != null)
                LanguageManager.Instance.OnLanguageChanged.AddListener(UpdateLocalizedText);
            
            UpdateLocalizedText();
            ApplyThemeStyle();
        }

        protected virtual void OnDisable()
        {
            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.UnregisterWidget(this);
            if (UIInputHandler.Instance != null && toggleKey != KeyCode.None) UIInputHandler.Instance.UnregisterToggleKey(toggleKey, this);

            UIThemeManager.OnThemeChanged -= ApplyThemeStyle;
            if (LanguageManager.Instance != null)
                LanguageManager.Instance.OnLanguageChanged.RemoveListener(UpdateLocalizedText);
            
            _isHovering = false;
            _isPressed = false;
        }

        protected virtual void OnDestroy()
        {
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            if (_colorFadeCoroutine != null) StopCoroutine(_colorFadeCoroutine);
            if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
        }
        
        protected virtual void ApplyThemeStyle()
        {
            if (UIThemeManager.Instance == null) return;

            var mainStyle = UIThemeManager.Instance.GetStyle(styleKey);
            if (mainStyle != null)
            {
                this.animationDuration = mainStyle.animationDuration;
                this.animationEaseType = mainStyle.animationEaseType;
                
                var mainTextComponent = GetComponentInChildren<TextMeshProUGUI>(true);
                if (mainTextComponent != null && mainTextComponent.GetComponent<StylableText>() == null)
                {
                    if (mainStyle.font != null) mainTextComponent.font = mainStyle.font;
                    mainTextComponent.fontStyle = mainStyle.fontStyle;
                    mainTextComponent.characterSpacing = mainStyle.characterSpacing;
                    mainTextComponent.wordSpacing = mainStyle.wordSpacing;
                    mainTextComponent.lineSpacing = mainStyle.lineSpacing;
                }
            }

            var stylableTexts = GetComponentsInChildren<StylableText>(true);
            foreach (var stylableText in stylableTexts)
            {
                if (!string.IsNullOrEmpty(stylableText.styleKey))
                {
                    var textStyle = UIThemeManager.Instance.GetStyle(stylableText.styleKey);
                    if (textStyle != null)
                    {
                        stylableText.ApplyStyle(textStyle);
                    }
                }
            }
            DoStateTransition(true);
        }

        protected virtual void DoStateTransition(bool instant)
        {
            // --- 1. Hauptgrafik aktualisieren ---
            var mainStyle = UIThemeManager.Instance?.GetStyle(styleKey);
            if (mainStyle != null && targetGraphic != null)
            {
                StyleState targetGraphicState = mainStyle.Normal;
                if (CurrentState == UIState.Disabled) targetGraphicState = mainStyle.Disabled;
                else if (CurrentState == UIState.Interactive)
                {
                    if (_isPressed) targetGraphicState = mainStyle.Pressed;
                    else if (_isHovering) targetGraphicState = mainStyle.Hover;
                }

                if (targetGraphic is Image image && targetGraphicState.sprite != null)
                    image.sprite = targetGraphicState.sprite;
                
                if (_colorFadeCoroutine != null) StopCoroutine(_colorFadeCoroutine);
                if (instant || mainStyle.graphicFadeDuration <= 0)
                    targetGraphic.color = targetGraphicState.color;
                else
                    _colorFadeCoroutine = StartCoroutine(FadeColor(targetGraphic, targetGraphicState.color, mainStyle.graphicFadeDuration));
            }

            // --- 2. KORREKTUR: Alle untergeordneten Texte aktualisieren ---
            var stylableTexts = GetComponentsInChildren<StylableText>(true);
            foreach (var stylableText in stylableTexts)
            {
                var textStyle = UIThemeManager.Instance.GetStyle(stylableText.styleKey);
                if (textStyle != null)
                {
                    Color targetTextColor = textStyle.textColorTints.normalColor;
                    if (CurrentState == UIState.Disabled) targetTextColor = textStyle.textColorTints.disabledColor;
                    else if (CurrentState == UIState.Interactive)
                    {
                        if (_isPressed) targetTextColor = textStyle.textColorTints.pressedColor;
                        else if (_isHovering) targetTextColor = textStyle.textColorTints.hoverColor;
                    }
                    stylableText.GetComponent<TextMeshProUGUI>().color = targetTextColor;
                }
            }
            
            // --- 3. Haupt-Textkomponente aktualisieren (falls vorhanden und nicht separat gestylt) ---
            if (_textComponent != null && _textComponent.GetComponent<StylableText>() == null && mainStyle != null)
            {
                Color targetTextColor = mainStyle.textColorTints.normalColor;
                if (CurrentState == UIState.Disabled) targetTextColor = mainStyle.textColorTints.disabledColor;
                else if (CurrentState == UIState.Interactive)
                {
                    if (_isPressed) targetTextColor = mainStyle.textColorTints.pressedColor;
                    else if (_isHovering) targetTextColor = mainStyle.textColorTints.hoverColor;
                }
                _textComponent.color = targetTextColor;
            }
        }

        protected virtual void UpdateLocalizedText()
        {
            if (_textComponent != null && !string.IsNullOrEmpty(_localizationKey) && LanguageManager.Instance != null)
            {
                _textComponent.text = LanguageManager.Instance.GetString(_localizationKey);
            }
        }

        public virtual void SetText(string text)
        {
            if (_textComponent == null) _textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
            if (_textComponent != null) _textComponent.text = text;
        }

        private IEnumerator FadeColor(Graphic target, Color targetColor, float duration)
        {
            float timer = 0f;
            Color startColor = target.color;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                target.color = Color.Lerp(startColor, targetColor, timer / duration);
                yield return null;
            }
            target.color = targetColor;
        }

        public virtual void Show() { if (IsVisible && gameObject.activeSelf) return; if (_animationCoroutine != null) StopCoroutine(_animationCoroutine); OnShowStart?.Invoke(); gameObject.SetActive(true); SetState(UIState.Interactive); if (focusOnShow && transform.parent != null) { transform.SetAsLastSibling(); } PlaySound(showSound); _animationCoroutine = StartCoroutine(AnimateShow()); }
        public virtual void Hide() { if (!IsVisible && !gameObject.activeSelf) return; if (_animationCoroutine != null) StopCoroutine(_animationCoroutine); OnHideStart?.Invoke(); SetState(UIState.NotInteractive); PlaySound(hideSound); _animationCoroutine = StartCoroutine(AnimateHide()); }
        private IEnumerator AnimateShow() { if ((transition & VisualTransition.Fade) != 0) _canvasGroup.alpha = 0f; if ((transition & VisualTransition.Slide) != 0) _rectTransform.anchoredPosition = _originalPosition + slideTransition.startOffset; if ((transition & VisualTransition.Scale) != 0) _rectTransform.localScale = Vector3.zero; if ((transition & VisualTransition.Fade) != 0) StartCoroutine(AnimateAlpha(1f, animationDuration, animationEaseType)); if ((transition & VisualTransition.Slide) != 0) TweenPosition(_originalPosition + slideTransition.endOffset, animationDuration, animationEaseType); if ((transition & VisualTransition.Scale) != 0) TweenScale(_originalScale, animationDuration, animationEaseType); yield return new WaitForSeconds(animationDuration); OnShowComplete?.Invoke(); _animationCoroutine = null; }
        private IEnumerator AnimateHide() { if ((transition & VisualTransition.Fade) != 0) StartCoroutine(AnimateAlpha(0f, animationDuration, animationEaseType)); if ((transition & VisualTransition.Slide) != 0) TweenPosition(_originalPosition + slideTransition.startOffset, animationDuration, animationEaseType); if ((transition & VisualTransition.Scale) != 0) TweenScale(Vector3.zero, animationDuration, animationEaseType); yield return new WaitForSeconds(animationDuration); OnHideComplete?.Invoke(); if (deactivateOnHide) { gameObject.SetActive(false); } _animationCoroutine = null; }
        public virtual void Toggle() { if (IsVisible) Hide(); else Show(); }
        public virtual void SetState(UIState newState) { CurrentState = newState; if (_canvasGroup != null) { _canvasGroup.interactable = (newState == UIState.Interactive); _canvasGroup.blocksRaycasts = (newState != UIState.Disabled); } DoStateTransition(false); }
        public Coroutine TweenPosition(Vector2 targetPosition, float duration, Easing.EaseType ease) { return StartCoroutine(AnimateVector2((v) => _rectTransform.anchoredPosition = v, _rectTransform.anchoredPosition, targetPosition, duration, ease)); }
        public Coroutine TweenScale(Vector3 targetScale, float duration, Easing.EaseType ease) { return StartCoroutine(AnimateVector3((v) => _rectTransform.localScale = v, _rectTransform.localScale, targetScale, duration, ease)); }
        public Coroutine TweenRotation(Quaternion targetRotation, float duration, Easing.EaseType ease) { return StartCoroutine(AnimateQuaternion((q) => _rectTransform.localRotation = q, _rectTransform.localRotation, targetRotation, duration, ease)); }
        public virtual void OnFocusGained() => OnFocusGainedEvent?.Invoke();
        public virtual void OnFocusLost() => OnFocusLostEvent?.Invoke();
        public virtual void OnPointerEnter(PointerEventData eventData) { _isHovering = true; if (CurrentState == UIState.Interactive) { PlaySound(hoverSound); OnHoverEvent?.Invoke(this, true); DoStateTransition(false); if (wobbleOnHover) { if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine); _wobbleCoroutine = StartCoroutine(AnimateWobble()); } if (UITooltipDisplay.Instance != null) UITooltipDisplay.Instance.ShowTooltip(this); } }
        public virtual void OnPointerExit(PointerEventData eventData) { _isHovering = false; if (CurrentState == UIState.Interactive) { OnHoverEvent?.Invoke(this, false); DoStateTransition(false); } if (UITooltipDisplay.Instance != null) UITooltipDisplay.Instance.HideTooltip(this); }
        public virtual void OnPointerDown(PointerEventData eventData) { if (CurrentState == UIState.Interactive) { _isPressed = true; OnPressEvent?.Invoke(this, true); DoStateTransition(false); } }
        public virtual void OnPointerUp(PointerEventData eventData) { if (CurrentState == UIState.Interactive) { _isPressed = false; OnPressEvent?.Invoke(this, false); DoStateTransition(false); } }
        public virtual void OnPointerClick(PointerEventData eventData) { if (CurrentState == UIState.Interactive) { PlaySound(clickSound); OnClickEvent?.Invoke(this, eventData); } }
        public virtual void OnBeginDrag(PointerEventData eventData) { if (!IsDraggable || CurrentState != UIState.Interactive) return; _isDragging = true; OnDragStartEvent?.Invoke(this, eventData); }
        public virtual void OnDrag(PointerEventData eventData) { if (!_isDragging) return; OnDragEvent?.Invoke(this, eventData); }
        public virtual void OnEndDrag(PointerEventData eventData) { if (!_isDragging) return; _isDragging = false; OnDragEndEvent?.Invoke(this, eventData); }
        private IEnumerator AnimateWobble() { yield return TweenRotation(Quaternion.Euler(0, 0, 5), 0.05f, Easing.EaseType.EaseOutQuad); yield return TweenRotation(Quaternion.Euler(0, 0, -5), 0.1f, Easing.EaseType.EaseOutQuad); yield return TweenRotation(Quaternion.Euler(0, 0, 0), 0.05f, Easing.EaseType.EaseOutQuad); _wobbleCoroutine = null; }
        protected IEnumerator AnimateAlpha(float targetAlpha, float duration, Easing.EaseType ease) { float startAlpha = _canvasGroup.alpha; float timer = 0f; while (timer < duration) { timer += Time.unscaledDeltaTime; float progress = Mathf.Clamp01(timer / duration); _canvasGroup.alpha = Easing.GetValue(ease, startAlpha, targetAlpha, progress); yield return null; } _canvasGroup.alpha = targetAlpha; }
        private IEnumerator AnimateVector3(Action<Vector3> setter, Vector3 start, Vector3 end, float duration, Easing.EaseType ease) { float timer = 0f; while (timer < duration) { timer += Time.unscaledDeltaTime; float progress = Mathf.Clamp01(timer / duration); float easedProgress = Easing.GetValue(ease, 0, 1, progress); setter(Vector3.LerpUnclamped(start, end, easedProgress)); yield return null; } setter(end); }
        private IEnumerator AnimateVector2(Action<Vector2> setter, Vector2 start, Vector2 end, float duration, Easing.EaseType ease) { float timer = 0f; while (timer < duration) { timer += Time.unscaledDeltaTime; float progress = Mathf.Clamp01(timer / duration); float easedProgress = Easing.GetValue(ease, 0, 1, progress); setter(Vector2.LerpUnclamped(start, end, easedProgress)); yield return null; } setter(end); }
        private IEnumerator AnimateQuaternion(Action<Quaternion> setter, Quaternion start, Quaternion end, float duration, Easing.EaseType ease) { float timer = 0f; while (timer < duration) { timer += Time.unscaledDeltaTime; float progress = Mathf.Clamp01(timer / duration); float easedProgress = Easing.GetValue(ease, 0, 1, progress); setter(Quaternion.SlerpUnclamped(start, end, easedProgress)); yield return null; } setter(end); }
        protected void PlaySound(AudioClip clip) { if (clip != null && Camera.main != null) { AudioSource audioSource = GetComponent<AudioSource>(); if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false; audioSource.spatialBlend = 0; } audioSource.PlayOneShot(clip); } }
        public void SetWidgetName(string newName) { widgetName = newName; }
        public void SetAnimationDuration(float duration) { animationDuration = duration; }
        public void SetDeactivateOnHide(bool deactivate) { deactivateOnHide = deactivate; }
        public void SetFocusOnShow(bool focus) { focusOnShow = focus; }
        public void SetToggleKey(KeyCode key) { toggleKey = key; }
        public void SetTargetGraphic(Graphic newTarget) { targetGraphic = newTarget; }
    }
}