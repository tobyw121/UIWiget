// Dateiname: UIThemeManager.cs
using UnityEngine;

/// <summary>
/// Verwaltet das aktive UI-Theme für das gesamte Spiel.
/// UIWidgets können auf dieses Theme zugreifen, um ein konsistentes Erscheinungsbild zu gewährleisten.
/// </summary>
public class UIThemeManager : MonoBehaviour
{
    public static UIThemeManager Instance { get; private set; }

    [Tooltip("Das UI-Theme, das standardmäßig für alle UIWidgets verwendet werden soll.")]
    public UIThemeData activeTheme;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}