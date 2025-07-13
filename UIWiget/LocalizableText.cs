using UnityEngine;
using TMPro;
using UnityEngine.UI; // Für die alte Text-Komponente
using YourGame.UI;

public class LocalizableText : MonoBehaviour
{
    public string translationKey; // Der Schlüssel in der translations.json
    private TMP_Text tmpTextComponent;    // TextMeshPro-Komponente (kann null sein)
    private Text legacyTextComponent;     // Legacy Text-Komponente (kann null sein)

    void Start()
    {
        // Versuche, *beide* Komponenten zu finden. Eine davon sollte vorhanden sein.
        tmpTextComponent = GetComponent<TMP_Text>();
        legacyTextComponent = GetComponent<Text>();

        if (tmpTextComponent == null && legacyTextComponent == null)
        {
            Debug.LogError("LocalizableText benötigt entweder eine TextMeshPro- oder eine Text-Komponente!", this); // Aussagekräftigere Fehlermeldung
            enabled = false; // Deaktiviere das Skript, um weitere Fehler zu vermeiden
            return;
        }

        UpdateText(); // Aktualisiere den Text beim Start.

        // Melde dich beim OnLanguageChanged-Event an.
        LanguageManager.Instance.OnLanguageChanged.AddListener(UpdateText);
    }

    // Wichtig: Entferne den Listener, wenn das Objekt zerstört wird, um Memory Leaks zu vermeiden.
    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged.RemoveListener(UpdateText);
        }
    }

    public void UpdateText()
    {
        if (string.IsNullOrEmpty(translationKey))
        {
            Debug.LogWarning("LocalizableText hat keinen translationKey gesetzt!", this);
            return;
        }

        string localizedString = LanguageManager.Instance.GetString(translationKey);

        if (tmpTextComponent != null)
        {
            tmpTextComponent.text = localizedString;
        }
        else if (legacyTextComponent != null)
        {
            legacyTextComponent.text = localizedString;
        }
    }
}