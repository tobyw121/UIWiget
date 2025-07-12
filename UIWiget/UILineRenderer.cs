// Dateiname: UILineRenderer.cs (Final Vereinfacht)
using UnityEngine;
using UnityEngine.UI;

// KORREKTUR: Erbt jetzt von MonoBehaviour statt UIWidget, um Konflikte zu vermeiden.
[RequireComponent(typeof(Image))]
public class UILineRenderer : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _lineImage;

    private void Awake()
    {
        // Wir holen uns die Referenzen direkt.
        _rectTransform = GetComponent<RectTransform>();
        _lineImage = GetComponent<Image>();
    }

    public void SetPositions(Vector2 startPos, Vector2 endPos)
    {
        if (_rectTransform == null)
        {
            Debug.LogError("UILineRenderer: RectTransform konnte nicht gefunden werden!", gameObject);
            return;
        }

        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.zero;

        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);

        _rectTransform.sizeDelta = new Vector2(distance, 5f);
        _rectTransform.anchoredPosition = startPos + direction * distance * 0.5f;
            
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
}