using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using YourGame.UI;
using YourGame.UI.Widgets.Animation;
using YourGame.UI.Widgets.Services;
using YourGame.UI.Widgets.Tooltips;

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
                case EaseType.EaseInQuad: return d * value * value + start;
                case EaseType.EaseOutQuad: return -d * value * (value - 2) + start;
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
        public enum UIState { Interactive, Disabled, NotInteractive, Loading }
        [System.Flags]
        public enum VisualTransition { None = 0, ColorTint = 1, SpriteSwap = 2, Fade = 4, Scale = 8, Slide = 16 }

        [Serializable]
        public class TooltipInfo
        {
            public bool Enabled = true;
            [TextArea] public string TooltipText = string.Empty;
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
        [SerializeField] public bool deactivateOnHide = true;
        [SerializeField] public bool focusOnShow = true;
        [SerializeField] public KeyCode toggleKey = KeyCode.None;

        [Header("Animation")]
        [SerializeField] public float animationDuration = 0.25f;
        [SerializeField] public Easing.EaseType animationEaseType = Easing.EaseType.EaseOutQuad;

        [Header("Visuals")]
        [SerializeField] private VisualTransition transition = VisualTransition.Fade;
        [SerializeField] public Graphic targetGraphic;
        [SerializeField] private GameObject loadingIndicator;
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
        [Tooltip("Das Widget, das standardmäßig fokussiert wird, wenn dieses Panel geöffnet wird.")]
        public UIWidget firstSelected;

        [Header("Localization")]
        [Tooltip("Der Lokalisierungs-Schlüssel aus translations.json.")]
        [SerializeField] private string _localizationKey = string.Empty;

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

        public static event Action<UIWidget> OnGlobalPointerEnter;
        public static event Action<UIWidget> OnGlobalPointerExit;

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

        public bool isHovering;
        public bool isPressed;
        public bool isDragging;

        public Vector2 _originalPosition;
        protected Vector3 _originalScale;

        private Coroutine _asyncOperationCoroutine;
        private CancellationTokenSource _currentAsyncCancellation;
        private UIWidgetAnimator _animator;
        private IThemeService _themeService;
        private ILocalizationService _localizationService;
        private ITooltipResolver _tooltipResolver;
        private bool _ownsThemeService = true;
        private bool _ownsLocalizationService = true;

        protected UIStyle AppliedStyle => string.IsNullOrEmpty(styleKey) ? null : _themeService?.GetStyle(styleKey);
        public UIStyle appliedStyle => AppliedStyle;

        internal CanvasGroup CanvasGroup => _canvasGroup;
        internal Vector2 OriginalAnchoredPosition => _originalPosition;
        internal Vector3 OriginalScale => _originalScale;

        public void RaiseGlobalPointerEnter() => OnGlobalPointerEnter?.Invoke(this);
        public void RaiseGlobalPointerExit() => OnGlobalPointerExit?.Invoke(this);

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
            _textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
            _originalPosition = _rectTransform.anchoredPosition;
            _originalScale = _rectTransform.localScale;
            EnsureDependencies();

            if (gameObject.activeInHierarchy)
            {
                SetState(startingState);
            }
            else
            {
                if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                SetState(UIState.Disabled);
                if (deactivateOnHide) gameObject.SetActive(false);
            }
        }

        protected virtual void OnEnable()
        {
            EnsureDependencies();

            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.RegisterWidget(this);
            if (toggleKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.RegisterToggleKey(toggleKey, this);
            }

            if (_themeService != null)
            {
                _themeService.ThemeChanged += ApplyThemeStyle;
            }

            if (_localizationService != null)
            {
                _localizationService.LanguageChanged += UpdateLocalizedText;
            }

            UpdateLocalizedText();
            ApplyThemeStyle();
        }

        protected virtual void OnDisable()
        {
            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.UnregisterWidget(this);
            if (UIInputHandler.Instance != null && toggleKey != KeyCode.None)
            {
                UIInputHandler.Instance.UnregisterToggleKey(toggleKey, this);
            }

            if (_themeService != null)
            {
                _themeService.ThemeChanged -= ApplyThemeStyle;
            }

            if (_localizationService != null)
            {
                _localizationService.LanguageChanged -= UpdateLocalizedText;
            }

            CancelAsyncOperation();
            isHovering = false;
            isPressed = false;
        }

        protected virtual void OnDestroy()
        {
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            if (_colorFadeCoroutine != null) StopCoroutine(_colorFadeCoroutine);
            if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
            if (_asyncOperationCoroutine != null) StopCoroutine(_asyncOperationCoroutine);
            CancelAsyncOperation();

            if (_ownsThemeService)
            {
                _themeService?.Dispose();
            }

            if (_ownsLocalizationService)
            {
                _localizationService?.Dispose();
            }

            _themeService = null;
            _localizationService = null;
        }

        protected virtual void OnValidate()
        {
            if (_animator == null)
            {
                _animator = new UIWidgetAnimator();
            }
            _animator.Configure(this, transition, slideTransition);
        }

        public string GetResolvedTooltipText()
        {
            return _tooltipResolver?.ResolveText(this) ?? string.Empty;
        }

        public void SetOnClickAsync(Func<Task> asyncAction)
        {
            if (asyncAction == null) return;
            SetOnClickAsync(ct => asyncAction(), null, default);
        }

        public void SetOnClickAsync(Func<CancellationToken, Task> asyncAction, Action<Exception> errorCallback = null, CancellationToken externalCancellation = default)
        {
            if (asyncAction == null) return;

            OnClickEvent.RemoveAllListeners();
            OnClickEvent.AddListener((widget, data) =>
            {
                if (_asyncOperationCoroutine == null && CurrentState == UIState.Interactive)
                {
                    _asyncOperationCoroutine = StartCoroutine(HandleAsyncAction(asyncAction, errorCallback, externalCancellation));
                }
            });
        }

        public void CancelAsyncOperation()
        {
            if (_currentAsyncCancellation != null && !_currentAsyncCancellation.IsCancellationRequested)
            {
                _currentAsyncCancellation.Cancel();
            }
        }

        private IEnumerator HandleAsyncAction(Func<CancellationToken, Task> asyncAction, Action<Exception> errorCallback, CancellationToken externalCancellation)
        {
            UIState previousState = CurrentState;
            SetState(UIState.Loading);

            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation))
            {
                _currentAsyncCancellation = linkedSource;
                Task task;
                try
                {
                    task = asyncAction(linkedSource.Token);
                }
                catch (Exception ex)
                {
                    HandleAsyncException(errorCallback, ex);
                    RestorePreviousState(previousState);
                    _asyncOperationCoroutine = null;
                    _currentAsyncCancellation = null;
                    yield break;
                }

                while (!task.IsCompleted)
                {
                    if (linkedSource.IsCancellationRequested)
                    {
                        RestorePreviousState(previousState);
                        _asyncOperationCoroutine = null;
                        _currentAsyncCancellation = null;
                        yield break;
                    }
                    yield return null;
                }

                _asyncOperationCoroutine = null;
                _currentAsyncCancellation = null;

                if (task.IsFaulted)
                {
                    HandleAsyncException(errorCallback, task.Exception?.GetBaseException() ?? task.Exception);
                }
                else if (task.IsCanceled || linkedSource.IsCancellationRequested)
                {
                    RestorePreviousState(previousState);
                    yield break;
                }
            }

            RestorePreviousState(previousState);
        }

        private void RestorePreviousState(UIState previousState)
        {
            if (CurrentState == UIState.Loading)
            {
                SetState(previousState);
            }
        }

        private void HandleAsyncException(Action<Exception> errorCallback, Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            errorCallback?.Invoke(exception);
            Debug.LogError($"[UIWidget] Asynchrone Aufgabe auf '{name}' ist fehlgeschlagen: {exception}");
        }

        private void EnsureDependencies()
        {
            if (_animator == null)
            {
                _animator = new UIWidgetAnimator();
            }

            if (_themeService == null)
            {
                _themeService = new ThemeServiceAdapter();
                _ownsThemeService = true;
            }

            if (_localizationService == null)
            {
                _localizationService = new LocalizationServiceAdapter();
                _ownsLocalizationService = true;
            }

            if (_tooltipResolver == null)
            {
                _tooltipResolver = new DefaultTooltipResolver(_themeService, _localizationService);
            }

            _animator.Configure(this, transition, slideTransition);
        }

        public void ConfigureDependencies(IThemeService themeService = null, ILocalizationService localizationService = null, ITooltipResolver tooltipResolver = null)
        {
            if (themeService != null)
            {
                if (_ownsThemeService)
                {
                    _themeService?.Dispose();
                }

                _themeService = themeService;
                _ownsThemeService = false;
            }
            else if (_themeService == null)
            {
                _ownsThemeService = true;
            }

            if (localizationService != null)
            {
                if (_ownsLocalizationService)
                {
                    _localizationService?.Dispose();
                }

                _localizationService = localizationService;
                _ownsLocalizationService = false;
            }
            else if (_localizationService == null)
            {
                _ownsLocalizationService = true;
            }

            if (tooltipResolver != null)
            {
                _tooltipResolver = tooltipResolver;
            }

            EnsureDependencies();
        }

        protected virtual void ApplyThemeStyle()
        {
            if (AppliedStyle != null)
            {
                animationDuration = AppliedStyle.animationDuration;
                animationEaseType = AppliedStyle.animationEaseType;
            }

            DoStateTransition(true);
        }

        protected virtual void DoStateTransition(bool instant)
        {
            var mainStyle = AppliedStyle;
            if (mainStyle == null) return;

            if (targetGraphic != null)
            {
                StyleState targetGraphicState = mainStyle.Normal;
                if (CurrentState == UIState.Disabled) targetGraphicState = mainStyle.Disabled;
                else if (CurrentState == UIState.Interactive)
                {
                    if (isPressed) targetGraphicState = mainStyle.Pressed;
                    else if (isHovering) targetGraphicState = mainStyle.Hover;
                }

                if (targetGraphic is Image image && targetGraphicState.sprite != null)
                {
                    image.sprite = targetGraphicState.sprite;
                }

                if (_colorFadeCoroutine != null)
                {
                    StopCoroutine(_colorFadeCoroutine);
                }

                if (instant || mainStyle.graphicFadeDuration <= 0)
                {
                    targetGraphic.color = targetGraphicState.color;
                }
                else
                {
                    _colorFadeCoroutine = StartCoroutine(FadeColor(targetGraphic, targetGraphicState.color, mainStyle.graphicFadeDuration));
                }
            }

            if (_textComponent != null && _textComponent.GetComponent<StylableText>() == null)
            {
                Color targetTextColor = mainStyle.textColorTints.normalColor;
                if (CurrentState == UIState.Disabled) targetTextColor = mainStyle.textColorTints.disabledColor;
                else if (CurrentState == UIState.Interactive)
                {
                    if (isPressed) targetTextColor = mainStyle.textColorTints.pressedColor;
                    else if (isHovering) targetTextColor = mainStyle.textColorTints.hoverColor;
                }

                _textComponent.color = targetTextColor;
            }
        }

        protected virtual void UpdateLocalizedText()
        {
            if (_textComponent == null || string.IsNullOrEmpty(_localizationKey))
            {
                return;
            }

            string localized = _localizationService?.GetString(_localizationKey);
            if (localized == null && LanguageManager.Instance != null)
            {
                localized = LanguageManager.Instance.GetString(_localizationKey);
            }

            localized ??= string.Empty;
            _textComponent.text = localized;
        }

        public virtual void SetText(string text)
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (_textComponent != null)
            {
                _textComponent.text = text;
            }
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

        public virtual void Show()
        {
            if (IsVisible && gameObject.activeSelf) return;
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);

            OnShowStart?.Invoke();
            gameObject.SetActive(true);
            SetState(UIState.Interactive);

            if (focusOnShow && transform.parent != null)
            {
                transform.SetAsLastSibling();
            }

            PlaySound(showSound);

            if (firstSelected != null && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.SetFocus(firstSelected);
            }

            _animationCoroutine = StartCoroutine(RunShowAnimation());
        }

        public virtual void Hide()
        {
            if (!IsVisible && !gameObject.activeSelf) return;
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);

            OnHideStart?.Invoke();
            SetState(UIState.NotInteractive);
            PlaySound(hideSound);
            _animationCoroutine = StartCoroutine(RunHideAnimation());
        }

        private IEnumerator RunShowAnimation()
        {
            if (_animator != null)
            {
                yield return _animator.PlayShow(this, animationDuration, animationEaseType);
            }

            OnShowComplete?.Invoke();
            _animationCoroutine = null;
        }

        private IEnumerator RunHideAnimation()
        {
            if (_animator != null)
            {
                yield return _animator.PlayHide(this, animationDuration, animationEaseType);
            }

            OnHideComplete?.Invoke();
            if (deactivateOnHide)
            {
                gameObject.SetActive(false);
            }

            _animationCoroutine = null;
        }

        public virtual void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        public virtual void SetState(UIState newState)
        {
            CurrentState = newState;
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = newState == UIState.Interactive;
                _canvasGroup.blocksRaycasts = newState != UIState.Disabled;
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(newState == UIState.Loading);
            }

            DoStateTransition(false);
        }

        public Coroutine TweenPosition(Vector2 targetPosition, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(WidgetAnimationUtility.AnimateVector2(v => _rectTransform.anchoredPosition = v, _rectTransform.anchoredPosition, targetPosition, duration, ease));
        }

        public Coroutine TweenScale(Vector3 targetScale, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(WidgetAnimationUtility.AnimateVector3(v => _rectTransform.localScale = v, _rectTransform.localScale, targetScale, duration, ease));
        }

        public Coroutine TweenRotation(Quaternion targetRotation, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(WidgetAnimationUtility.AnimateQuaternion(q => _rectTransform.localRotation = q, _rectTransform.localRotation, targetRotation, duration, ease));
        }

        public virtual void OnFocusGained()
        {
            OnFocusGainedEvent?.Invoke();
            if (targetGraphic != null)
            {
                transform.localScale = _originalScale * 1.05f;
            }
        }

        public virtual void OnFocusLost()
        {
            OnFocusLostEvent?.Invoke();
            if (targetGraphic != null)
            {
                transform.localScale = _originalScale;
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            OnGlobalPointerEnter?.Invoke(this);

            if (CurrentState == UIState.Interactive)
            {
                PlaySound(hoverSound);
                OnHoverEvent?.Invoke(this, true);
                DoStateTransition(false);

                if (wobbleOnHover)
                {
                    if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
                    _wobbleCoroutine = StartCoroutine(AnimateWobble());
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            OnGlobalPointerExit?.Invoke(this);
            if (CurrentState == UIState.Interactive)
            {
                OnHoverEvent?.Invoke(this, false);
                DoStateTransition(false);
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                isPressed = true;
                OnPressEvent?.Invoke(this, true);
                DoStateTransition(false);
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                isPressed = false;
                OnPressEvent?.Invoke(this, false);
                DoStateTransition(false);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                PlaySound(clickSound);
                OnClickEvent?.Invoke(this, eventData);
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable || CurrentState != UIState.Interactive)
            {
                return;
            }

            isDragging = true;
            OnDragStartEvent?.Invoke(this, eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            OnDragEvent?.Invoke(this, eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            OnDragEndEvent?.Invoke(this, eventData);
        }

        private IEnumerator AnimateWobble()
        {
            yield return TweenRotation(Quaternion.Euler(0, 0, 5), 0.05f, Easing.EaseType.EaseOutQuad);
            yield return TweenRotation(Quaternion.Euler(0, 0, -5), 0.1f, Easing.EaseType.EaseOutQuad);
            yield return TweenRotation(Quaternion.Euler(0, 0, 0), 0.05f, Easing.EaseType.EaseOutQuad);
            _wobbleCoroutine = null;
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip != null && Camera.main != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                }

                audioSource.PlayOneShot(clip);
            }
        }

        public void SetWidgetName(string newName) => widgetName = newName;
        public void SetTargetGraphic(Graphic newTarget) => targetGraphic = newTarget;
        public void SetAnimationDuration(float duration) => animationDuration = duration;
        public void SetDeactivateOnHide(bool value) => deactivateOnHide = value;
        public void SetToggleKey(KeyCode key) => toggleKey = key;
        public void SetFocusOnShow(bool value) => focusOnShow = value;
    }
}
