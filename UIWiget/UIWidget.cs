// Dateiname: UIWidget.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    // Hilfsklasse für Easing-Funktionen
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
                default: // Linear
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
        }

        [Serializable]
        public class ColorTintBlock
        {
            public Color normalColor = Color.white;
            public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            public Color pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            public float fadeDuration = 0.1f;
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

        [Header("Navigation (Keyboard/Gamepad)")]
        [Tooltip("Das Widget, das fokussiert wird, wenn die 'Nach-Oben'-Taste gedrückt wird.")]
        public UIWidget selectOnUp;
        [Tooltip("Das Widget, das fokussiert wird, wenn die 'Nach-Unten'-Taste gedrückt wird.")]
        public UIWidget selectOnDown;
        [Tooltip("Das Widget, das fokussiert wird, wenn die 'Nach-Links'-Taste gedrückt wird.")]
        public UIWidget selectOnLeft;
        [Tooltip("Das Widget, das fokussiert wird, wenn die 'Nach-Rechts'-Taste gedrückt wird.")]
        public UIWidget selectOnRight;
        
        [Header("Animation")]
        [SerializeField] protected float animationDuration = 0.25f;
        [SerializeField] protected Easing.EaseType animationEaseType = Easing.EaseType.EaseOutQuad;
        
        [Header("Visuals")]
        [SerializeField] private VisualTransition transition = VisualTransition.Fade;
        [SerializeField] protected Graphic targetGraphic;
        
        [Tooltip("Wenn hier ein Theme zugewiesen wird, überschreibt es das globale Theme des UIThemeManagers.")]
        [SerializeField] private UIThemeData themeOverride;
        
        [SerializeField] private ColorTintBlock colorTints = new ColorTintBlock();
        [SerializeField] private SlideTransition slideTransition = new SlideTransition();
        
        [Tooltip("Wackelt bei Mausberührung.")]
        [SerializeField] private bool wobbleOnHover = false;
        
        [Header("Sound Events")]
        [SerializeField] protected AudioClip showSound;
        [SerializeField] protected AudioClip hideSound;
        [SerializeField] protected AudioClip clickSound;
        [SerializeField] protected AudioClip hoverSound;
        [Header("Tooltip")]
        [SerializeField] public TooltipInfo Tooltip;
        
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
        
        public string Name
        {
            get => widgetName;
            set => widgetName = value;
        }

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
        private Vector2 _originalPosition;
        private Vector3 _originalScale;
        protected UIThemeData _activeTheme;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
            
            ApplyTheme();

            _originalPosition = _rectTransform.anchoredPosition;
            _originalScale = _rectTransform.localScale;
            
            if (gameObject.activeInHierarchy)
            {
                SetState(startingState);
            }
            else
            {
                if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                SetState(UIState.Disabled);
                if (deactivateOnHide)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        protected virtual void OnEnable()
        {
            ApplyTheme();
            
            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.RegisterWidget(this);
            if (toggleKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.RegisterToggleKey(toggleKey, this);
            }
            DoColorTransition(CurrentState, true);
        }

        protected virtual void OnDisable()
        {
            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.UnregisterWidget(this);
            if (UIInputHandler.Instance != null && toggleKey != KeyCode.None) UIInputHandler.Instance.UnregisterToggleKey(toggleKey, this);

            _isHovering = false;
            _isPressed = false;
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

            _animationCoroutine = StartCoroutine(AnimateShow());
        }

        public virtual void Hide()
        {
            if (!IsVisible && !gameObject.activeSelf) return;
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);

            OnHideStart?.Invoke();
            SetState(UIState.NotInteractive);
            PlaySound(hideSound);

            _animationCoroutine = StartCoroutine(AnimateHide());
        }

        private IEnumerator AnimateShow()
        {
            if ((transition & VisualTransition.Fade) != 0) _canvasGroup.alpha = 0f;
            if ((transition & VisualTransition.Slide) != 0) _rectTransform.anchoredPosition = _originalPosition + slideTransition.startOffset;
            if ((transition & VisualTransition.Scale) != 0) _rectTransform.localScale = Vector3.zero;
            
            if ((transition & VisualTransition.Fade) != 0)
                StartCoroutine(AnimateAlpha(1f, animationDuration, animationEaseType));
            if ((transition & VisualTransition.Slide) != 0)
                TweenPosition(_originalPosition + slideTransition.endOffset, animationDuration, animationEaseType);
            if ((transition & VisualTransition.Scale) != 0)
                TweenScale(_originalScale, animationDuration, animationEaseType);
            
            yield return new WaitForSeconds(animationDuration);

            OnShowComplete?.Invoke();
            _animationCoroutine = null;
        }

        private IEnumerator AnimateHide()
        {
            if ((transition & VisualTransition.Fade) != 0)
                StartCoroutine(AnimateAlpha(0f, animationDuration, animationEaseType));
            if ((transition & VisualTransition.Slide) != 0)
                TweenPosition(_originalPosition + slideTransition.startOffset, animationDuration, animationEaseType);
            if ((transition & VisualTransition.Scale) != 0)
                TweenScale(Vector3.zero, animationDuration, animationEaseType);
            
            yield return new WaitForSeconds(animationDuration);

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
                _canvasGroup.interactable = (newState == UIState.Interactive);
                _canvasGroup.blocksRaycasts = (newState != UIState.Disabled);
            }
            DoColorTransition(CurrentState, false);
        }

        public Coroutine TweenPosition(Vector2 targetPosition, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(AnimateVector2((v) => _rectTransform.anchoredPosition = v, _rectTransform.anchoredPosition, targetPosition, duration, ease));
        }

        public Coroutine TweenScale(Vector3 targetScale, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(AnimateVector3((v) => _rectTransform.localScale = v, _rectTransform.localScale, targetScale, duration, ease));
        }

        public Coroutine TweenRotation(Quaternion targetRotation, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(AnimateQuaternion((q) => _rectTransform.localRotation = q, _rectTransform.localRotation, targetRotation, duration, ease));
        }

        public virtual void OnFocusGained() => OnFocusGainedEvent?.Invoke();
        public virtual void OnFocusLost() => OnFocusLostEvent?.Invoke();
        
        public virtual void SetText(string text)
        {
            TMPro.TextMeshProUGUI tmpText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null) tmpText.text = text;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            if (CurrentState == UIState.Interactive)
            {
                PlaySound(hoverSound);
                OnHoverEvent?.Invoke(this, true);
                DoColorTransition(CurrentState, false);

                if (wobbleOnHover)
                {
                   TriggerWobble();
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            if (CurrentState == UIState.Interactive)
            {
                OnHoverEvent?.Invoke(this, false);
                DoColorTransition(CurrentState, false);
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                _isPressed = true;
                OnPressEvent?.Invoke(this, true);
                DoColorTransition(CurrentState, false);
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                _isPressed = false;
                OnPressEvent?.Invoke(this, false);
                DoColorTransition(CurrentState, false);
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
            _isDragging = true;
            OnDragStartEvent?.Invoke(this, eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(this, eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            OnDragEndEvent?.Invoke(this, eventData);
        }
        
        public void SetWidgetName(string newName)
        {
            widgetName = newName;
        }

        public void SetAnimationDuration(float duration)
        {
            animationDuration = duration;
        }
        
        public void SetTargetGraphic(Graphic graphic)
        {
            this.targetGraphic = graphic;
            if (gameObject.activeInHierarchy)
            {
                DoColorTransition(CurrentState, true);
            }
        }

        public void TriggerWobble()
        {
            if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
            _wobbleCoroutine = StartCoroutine(AnimateWobble());
        }

        public void SetDeactivateOnHide(bool deactivate)
        {
            deactivateOnHide = deactivate;
        }

        public void SetFocusOnShow(bool focus)
        {
            focusOnShow = focus;
        }

        public void SetToggleKey(KeyCode key)
        {
            if (toggleKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.UnregisterToggleKey(toggleKey, this);
            }
            
            toggleKey = key;
            if (toggleKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.RegisterToggleKey(toggleKey, this);
            }
        }

        private void ApplyTheme()
        {
            _activeTheme = themeOverride;
            if (_activeTheme == null && UIThemeManager.Instance != null)
            {
                _activeTheme = UIThemeManager.Instance.activeTheme;
            }

            if (_activeTheme != null && targetGraphic != null)
            {
                var textComponent = GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null && _activeTheme.mainFont != null)
                {
                    textComponent.font = _activeTheme.mainFont;
                    textComponent.color = _activeTheme.fontColor;
                }
            }
        }
        
        private IEnumerator AnimateWobble()
        {
            yield return TweenRotation(Quaternion.Euler(0, 0, 5), 0.05f, Easing.EaseType.EaseOutQuad);
            yield return TweenRotation(Quaternion.Euler(0, 0, -5), 0.1f, Easing.EaseType.EaseOutQuad);
            yield return TweenRotation(Quaternion.Euler(0, 0, 0), 0.05f, Easing.EaseType.EaseOutQuad);
            _wobbleCoroutine = null;
        }

        protected IEnumerator AnimateAlpha(float targetAlpha, float duration, Easing.EaseType ease)
        {
            float startAlpha = _canvasGroup.alpha;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                _canvasGroup.alpha = Easing.GetValue(ease, startAlpha, targetAlpha, progress);
                yield return null;
            }
            _canvasGroup.alpha = targetAlpha;
        }

        private IEnumerator AnimateVector3(Action<Vector3> setter, Vector3 start, Vector3 end, float duration, Easing.EaseType ease)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedProgress = Easing.GetValue(ease, 0, 1, progress);
                setter(Vector3.LerpUnclamped(start, end, easedProgress));
                yield return null;
            }
            setter(end);
        }

        private IEnumerator AnimateVector2(Action<Vector2> setter, Vector2 start, Vector2 end, float duration, Easing.EaseType ease)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedProgress = Easing.GetValue(ease, 0, 1, progress);
                setter(Vector2.LerpUnclamped(start, end, easedProgress));
                yield return null;
            }
            setter(end);
        }

        private IEnumerator AnimateQuaternion(Action<Quaternion> setter, Quaternion start, Quaternion end, float duration, Easing.EaseType ease)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedProgress = Easing.GetValue(ease, 0, 1, progress);
                setter(Quaternion.SlerpUnclamped(start, end, easedProgress));
                yield return null;
            }
            setter(end);
        }

        protected virtual void DoColorTransition(UIState state, bool instant)
        {
            ColorTintBlock colors = (_activeTheme != null && _activeTheme.normalColor != null) ? _activeTheme.GetColorTintBlock() : this.colorTints;
            
            if (targetGraphic == null || (transition & VisualTransition.ColorTint) == 0) return;

            Color targetColor = colors.normalColor;
            if (state == UIState.Disabled)
            {
                targetColor = colors.disabledColor;
            }
            else if (state == UIState.Interactive)
            {
                if (_isPressed) targetColor = colors.pressedColor;
                else if (_isHovering) targetColor = colors.hoverColor;
            }

            if (_colorFadeCoroutine != null) StopCoroutine(_colorFadeCoroutine);
            
            float duration = instant ? 0f : colors.fadeDuration;
            if (duration <= 0f)
            {
                targetGraphic.color = targetColor;
            }
            else
            {
                _colorFadeCoroutine = StartCoroutine(FadeColor(targetColor, duration));
            }
        }

        private IEnumerator FadeColor(Color targetColor, float duration)
        {
            float timer = 0f;
            Color startColor = targetGraphic.color;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                targetGraphic.color = Color.Lerp(startColor, targetColor, timer / duration);
                yield return null;
            }
            targetGraphic.color = targetColor;
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
                    audioSource.spatialBlend = 0; // 2D-Sound
                }
                audioSource.PlayOneShot(clip);
            }
        }
    }
}