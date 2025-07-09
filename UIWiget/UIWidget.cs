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
        public enum EaseType { Linear, EaseInQuad, EaseOutQuad, EaseInOutQuad }
        public static float GetValue(EaseType easeType, float start, float end, float value)
        {
            switch (easeType)
            {
                case EaseType.EaseInQuad:
                    return (end - start) * value * value + start;
                case EaseType.EaseOutQuad:
                    return -(end - start) * value * (value - 2) + start;
                case EaseType.EaseInOutQuad:
                {
                    value /= .5f;
                    if (value < 1) return (end - start) * 0.5f * value * value + start;
                    value--;
                    return -(end - start) * 0.5f * (value * (value - 2) - 1) + start;
                }
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
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDoubleClickHandler, IPointerRightClickHandler
    {
        public enum UIState { Interactive, Disabled, NotInteractive }
        public enum VisualTransition { None, ColorTint, SpriteSwap, Fade, Scale, Slide }

        [Serializable]
        public class TooltipInfo
        {
            public bool Enabled = true;
            [TextArea] public string TooltipText = "";
            public float Delay = 0.5f;
            public Vector2 Offset = new Vector2(0, -30);
            public bool ShowUI = true;
            public AudioClip Sound;
            public Vector2 Padding = new Vector2(10, 10);
            public Sprite BackgroundSprite;
            public Color Color = Color.white;
            public Color TextColor = Color.white;
            public float InitialAlpha = 0.1f;
            public float InitialScale = 0.1f;
            public float FinalScale = 1f;
            public Easing.EaseType Style = Easing.EaseType.Linear;
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
        [Header("Animation")]
        [SerializeField] protected float animationDuration = 0.25f;
        [SerializeField] protected Easing.EaseType animationEaseType = Easing.EaseType.EaseOutQuad;
        [Header("Visuals")]
        [SerializeField] private VisualTransition transition = VisualTransition.None;
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private ColorTintBlock colorTints = new ColorTintBlock();
        [SerializeField] private SlideTransition slideTransition = new SlideTransition();
        [Header("Sound Events")]
        [SerializeField] protected AudioClip showSound;
        [SerializeField] protected AudioClip hideSound;
        [SerializeField] protected AudioClip clickSound;
        [SerializeField] protected AudioClip hoverSound;
        [SerializeField] protected AudioClip doubleClickSound;
        [SerializeField] protected AudioClip rightClickSound;
        [Header("Tooltip")]
        [SerializeField] public TooltipInfo Tooltip;
        public UnityEvent OnShowStart, OnShowComplete, OnHideStart, OnHideComplete;
        public UnityEvent OnFocusGainedEvent, OnFocusLostEvent;
        public UnityEvent<UIWidget, PointerEventData> OnClickEvent;
        public UnityEvent<UIWidget, bool> OnPressEvent;
        public UnityEvent<UIWidget, bool> OnHoverEvent;
        public UnityEvent<UIWidget, PointerEventData> OnDragStartEvent;
        public UnityEvent<UIWidget, PointerEventData> OnDragEvent;
        public UnityEvent<UIWidget, PointerEventData> OnDragEndEvent;
        public UnityEvent<UIWidget, PointerEventData> OnDoubleClickEvent;
        public UnityEvent<UIWidget, PointerEventData> OnRightClickEvent;

        public string Name 
        { 
            get => widgetName;
            set => widgetName = value; // Setter hinzugefügt
        }

        public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0.99f;
        public UIState CurrentState { get; private set; }
        public object UserData { get; set; }
        public object ParentMenu { get; internal set; }
        public UIWidget ParentWidget { get; internal set; }
        public RectTransform RectTransform => _rectTransform;
        public bool IsVisibleInHierarchy { get; protected set; }
        public UIState StateInHierarchy { get; protected set; }

        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;
        private Coroutine _animationCoroutine;
        private Coroutine _colorFadeCoroutine;
        protected bool _isHovering;
        protected bool _isPressed;
        protected bool _isDragging;
        protected Image _imageComponent;
        protected RawImage _rawImageComponent;
        protected Text _textComponent; // Legacy Unity UI Text
        protected Collider _colliderComponent;
        private float _doubleClickTimeThreshold = 0.3f;
        private float _lastClickTime;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _imageComponent = GetComponent<Image>();
            _rawImageComponent = GetComponent<RawImage>();
            _textComponent = GetComponentInChildren<Text>();
            // If you only use TextMeshPro, this would be:
            // _textComponentTMP = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            _colliderComponent = GetComponent<Collider>();
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();

            if (gameObject.activeSelf)
            {
                SetState(startingState);
            }
            else
            {
                _canvasGroup.alpha = 0f;
                SetState(UIState.Disabled); // Startet im deaktivierten Zustand, wenn inaktiv im Editor
                if (deactivateOnHide)
                {
                    gameObject.SetActive(false); // Stellt sicher, dass es deaktiviert ist, wenn es versteckt ist
                }
            }
            UpdateColliderState();
        }

        protected virtual void OnEnable()
        {
            if(UIWidgetManager.Instance != null) UIWidgetManager.Instance.RegisterWidget(this);
            if (toggleKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.RegisterToggleKey(toggleKey, this);
            }
            DoColorTransition(CurrentState, true);
            UpdateColliderState();
        }

        protected virtual void OnDisable()
        {
            if (UIWidgetManager.Instance != null) UIWidgetManager.Instance.UnregisterWidget(this);
            if (UIInputHandler.Instance != null && toggleKey != KeyCode.None) UIInputHandler.Instance.UnregisterToggleKey(toggleKey, this);
            if (_isHovering) { OnHoverEvent?.Invoke(this, false); _isHovering = false; }
            if (_isPressed) { OnPressEvent?.Invoke(this, false); _isPressed = false; }
            UpdateColliderState();
        }

        public virtual void Show()
        {
            if (IsVisible && gameObject.activeSelf) return;
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            OnShowStart?.Invoke();
            gameObject.SetActive(true);
            SetState(UIState.Interactive);
            UpdateVisibleInHierarchy(true);
            if (focusOnShow && transform.parent != null)
            {
                transform.SetAsLastSibling(); // Bringt das Widget in der Hierarchie ganz nach vorne
            }
            PlaySound(showSound);
            _animationCoroutine = StartCoroutine(AnimateAlpha(1f, () => OnShowComplete?.Invoke()));
        }

        public virtual void Hide()
        {
            if (!IsVisible && !gameObject.activeSelf) return;
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            OnHideStart?.Invoke();
            SetState(UIState.NotInteractive); // Setzt auf NotInteractive, um Interaktionen während des Ausblendens zu verhindern
            UpdateVisibleInHierarchy(false);
            PlaySound(hideSound);
            _animationCoroutine = StartCoroutine(AnimateAlpha(0f, () => OnHideComplete?.Invoke()));
        }

        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        public virtual void SetState(UIState newState)
        {
            CurrentState = newState;
            UpdateStateInHierarchy(newState);
            switch (newState)
            {
                case UIState.Interactive:
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true; // WICHTIG: Blockiert Raycasts, damit Klicks nicht durchgehen
                    break;
                case UIState.NotInteractive:
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = true; // Blockiert Raycasts, aber nicht interaktiv
                    break;
                case UIState.Disabled:
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false; // Lässt Raycasts durch
                    break;
            }
            DoColorTransition(newState, false);
            UpdateColliderState();
        }

        public Coroutine TweenPosition(Vector2 targetPosition, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(AnimateVector2((v) => _rectTransform.anchoredPosition = v, _rectTransform.anchoredPosition, targetPosition, duration, ease));
        }

        public Coroutine TweenScale(Vector3 targetScale, float duration, Easing.EaseType ease)
        {
            return StartCoroutine(AnimateVector3((v) => _rectTransform.localScale = v, _rectTransform.localScale, targetScale, duration, ease));
        }

        public virtual void OnFocusGained()
        {
            OnFocusGainedEvent?.Invoke();
        }

        public virtual void OnFocusLost()
        {
            OnFocusLostEvent?.Invoke();
        }

        public virtual void SetText(string text)
        {
            TMPro.TextMeshProUGUI tmpText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null) tmpText.text = text;
            else if (_textComponent != null) _textComponent.text = text;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            if (CurrentState == UIState.Interactive)
            {
                PlaySound(hoverSound);
                OnHoverEvent?.Invoke(this, true);
                DoColorTransition(CurrentState, false);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (_isHovering && CurrentState == UIState.Interactive)
            {
                OnHoverEvent?.Invoke(this, false);
            }
            _isHovering = false;
            DoColorTransition(CurrentState, false);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            if (CurrentState == UIState.Interactive)
            {
                OnPressEvent?.Invoke(this, true);
                DoColorTransition(CurrentState, false);
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            if (CurrentState == UIState.Interactive)
            {
                OnPressEvent?.Invoke(this, false);
                DoColorTransition(CurrentState, false);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                float currentTime = Time.time;
                if (currentTime - _lastClickTime < _doubleClickTimeThreshold)
                {
                    PlaySound(doubleClickSound);
                    OnDoubleClickEvent?.Invoke(this, eventData);
                }
                else
                {
                    PlaySound(clickSound);
                    OnClickEvent?.Invoke(this, eventData);
                }
                _lastClickTime = currentTime;
            }
        }

        public virtual void OnPointerDoubleClick(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                PlaySound(doubleClickSound);
                OnDoubleClickEvent?.Invoke(this, eventData);
            }
        }

        public virtual void OnPointerRightClick(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                PlaySound(rightClickSound);
                OnRightClickEvent?.Invoke(this, eventData);
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

        public void OnParentSetVisible(bool parentVisible)
        {
            UpdateVisibleInHierarchy(parentVisible);
        }

        public void OnParentSetState(UIState parentState)
        {
            UpdateStateInHierarchy(parentState);
        }

        protected virtual void UpdateVisibleInHierarchy(bool parentVisible)
        {
            bool newVisibleInHierarchy = parentVisible && gameObject.activeSelf && _canvasGroup.alpha > 0.01f;
            if (IsVisibleInHierarchy != newVisibleInHierarchy)
            {
                IsVisibleInHierarchy = newVisibleInHierarchy;
                _canvasGroup.blocksRaycasts = IsVisibleInHierarchy; // Wichtig: Raycasts blockieren, wenn sichtbar
                foreach (Transform child in transform)
                {
                    UIWidget childWidget = child.GetComponent<UIWidget>();
                    if (childWidget != null)
                    {
                        childWidget.OnParentSetVisible(IsVisibleInHierarchy);
                    }
                }
            }
            UpdateColliderState();
        }

        protected virtual void UpdateStateInHierarchy(UIState parentState)
        {
            UIState newStateInHierarchy = CurrentState;
            if (parentState == UIState.Disabled || parentState == UIState.NotInteractive)
            {
                newStateInHierarchy = parentState;
            }
            if (StateInHierarchy != newStateInHierarchy)
            {
                StateInHierarchy = newStateInHierarchy;
                _canvasGroup.interactable = (StateInHierarchy == UIState.Interactive);
                _canvasGroup.blocksRaycasts = (StateInHierarchy != UIState.Disabled); // Blockiert Raycasts auch bei NotInteractive
                foreach (Transform child in transform)
                {
                    UIWidget childWidget = child.GetComponent<UIWidget>();
                    if (childWidget != null)
                    {
                        childWidget.OnParentSetState(StateInHierarchy);
                    }
                }
            }
            UpdateColliderState();
        }

        protected virtual void UpdateColliderState()
        {
            if (_colliderComponent != null)
            {
                _colliderComponent.enabled = (StateInHierarchy == UIState.Interactive) && IsVisibleInHierarchy;
            }
        }

        protected IEnumerator AnimateAlpha(float targetAlpha, Action onComplete = null)
        {
            float startAlpha = _canvasGroup.alpha;
            float timer = 0f;
            if (animationDuration <= 0f)
            {
                _canvasGroup.alpha = targetAlpha;
            }
            else
            {
                while (timer < animationDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(timer / animationDuration);
                    _canvasGroup.alpha = Easing.GetValue(animationEaseType, startAlpha, targetAlpha, progress);
                    yield return null;
                }
            }
            _canvasGroup.alpha = targetAlpha;
            if (targetAlpha < 0.01f && deactivateOnHide)
            {
                gameObject.SetActive(false);
            }
            _animationCoroutine = null;
            onComplete?.Invoke();
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

        protected virtual void DoColorTransition(UIState state, bool instant)
        {
            if (targetGraphic == null || transition != VisualTransition.ColorTint) return;
            Color targetColor = colorTints.normalColor;
            if (state == UIState.Disabled)
            {
                targetColor = colorTints.disabledColor;
            }
            else if (state == UIState.Interactive)
            {
                if (_isPressed) targetColor = colorTints.pressedColor;
                else if (_isHovering) targetColor = colorTints.hoverColor;
            }
            if (_colorFadeCoroutine != null) StopCoroutine(_colorFadeCoroutine);
            if (instant || colorTints.fadeDuration <= 0)
            {
                targetGraphic.color = targetColor;
            }
            else
            {
                _colorFadeCoroutine = StartCoroutine(FadeColor(targetColor, colorTints.fadeDuration));
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
                // Eine vorhandene AudioSource suchen oder eine temporäre erstellen
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

    public interface IPointerDoubleClickHandler : IEventSystemHandler
    {
        void OnPointerDoubleClick(PointerEventData eventData);
    }

    public interface IPointerRightClickHandler : IEventSystemHandler
    {
        void OnPointerRightClick(PointerEventData eventData);
    }
}