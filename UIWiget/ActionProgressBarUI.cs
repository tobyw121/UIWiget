// Dateiname: ActionProgressBarUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using YourGame.UI.Widgets;

// ActionProgressBarUI erbt jetzt direkt von UIWidget
public class ActionProgressBarUI : UIWidget
{
    public static ActionProgressBarUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject progressBarPanel;
    public Slider progressBarSlider;
    public TextMeshProUGUI progressText;
    public Image spinningIcon;

    [Header("Progress Bar Settings")]
    public float spinningSpeed = 200f;

    private Coroutine _currentProgressBarCoroutine;
    private Coroutine _spinningIconCoroutine;

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (progressBarPanel == null) progressBarPanel = gameObject;

        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Sicherstellen, dass die Fortschrittsanzeige bei Aktivierung immer auf 0 steht
        if (progressBarSlider != null)
        {
            progressBarSlider.value = 0f;
        }
        if (progressText != null)
        {
            progressText.text = "0%";
        }
        
        // Starten der Spinning-Animation, wenn das Panel aktiv wird
        if (spinningIcon != null)
        {
            if (_spinningIconCoroutine != null) StopCoroutine(_spinningIconCoroutine);
            _spinningIconCoroutine = StartCoroutine(SpinningIconRoutine());
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Stoppen der Spinning-Animation, wenn das Panel deaktiviert wird
        if (_spinningIconCoroutine != null)
        {
            StopCoroutine(_spinningIconCoroutine);
            _spinningIconCoroutine = null;
        }
        if (_currentProgressBarCoroutine != null)
        {
            StopCoroutine(_currentProgressBarCoroutine);
            _currentProgressBarCoroutine = null;
        }
    }

    public override void Show()
    {
        if (progressBarPanel == null || progressBarSlider == null)
        {
            Debug.LogError("Progress Bar UI-Referenzen sind im Inspector nicht zugewiesen!");
            return;
        }

        if (_currentProgressBarCoroutine != null)
        {
            StopCoroutine(_currentProgressBarCoroutine);
        }
        
        base.Show(); 
        
        // Stellen Sie sicher, dass animationDuration einen sinnvollen Wert hat, wenn Show() direkt aufgerufen wird
        // oder wenn Show(duration) nicht verwendet wird.
        if (this.animationDuration <= 0.01f)
        {
            this.animationDuration = 0.3f; // Standard-Fallback
        }

        _currentProgressBarCoroutine = StartCoroutine(AnimateProgressBar(this.animationDuration));
    }

    public override void Hide()
    {
        if (_currentProgressBarCoroutine != null)
        {
            StopCoroutine(_currentProgressBarCoroutine);
            _currentProgressBarCoroutine = null;
        }
        base.Hide();
    }

    // Eine überladene Show-Methode, um eine spezifische Dauer zu übergeben
    public void Show(float duration)
    {
        this.animationDuration = duration;
        Show();
    }

    private IEnumerator AnimateProgressBar(float duration)
    {
        float elapsedTime = 0f;
        progressBarSlider.value = 0f;
        if (progressText != null) progressText.text = "0%";

        Debug.Log($"[ActionProgressBarUI] Starte Fortschrittsanimation mit Dauer: {duration} Sekunden.");

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            progressBarSlider.value = progress;
            if (progressText != null) progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
            yield return null;
        }

        progressBarSlider.value = 1f;
        if (progressText != null) progressText.text = "100%";
        Debug.Log("[ActionProgressBarUI] Fortschrittsanimation abgeschlossen.");
    }

    private IEnumerator SpinningIconRoutine()
    {
        if (spinningIcon == null) yield break;

        while (true)
        {
            spinningIcon.rectTransform.Rotate(0, 0, -spinningSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }
}