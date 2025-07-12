// Dateiname: UITooltipDisplay.cs
using UnityEngine;
using System.Collections;
using YourGame.UI.Widgets;

namespace YourGame.UI
{
    public class UITooltipDisplay : MonoBehaviour
    {
        public static UITooltipDisplay Instance { get; private set; }
        
        [SerializeField] private UIWidget tooltipWidget; // Ein UIWidget, das als Tooltip-Template dient
        private Coroutine _showCoroutine;
        private UIWidget _currentTarget;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            if (tooltipWidget != null) tooltipWidget.gameObject.SetActive(false);
        }

        public void ShowTooltip(UIWidget target)
        {
            if (target == null || !target.Tooltip.Enabled || string.IsNullOrEmpty(target.Tooltip.TooltipText)) return;
            
            _currentTarget = target;
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ShowTooltipRoutine(target));
        }

        private IEnumerator ShowTooltipRoutine(UIWidget target)
        {
            yield return new WaitForSecondsRealtime(target.Tooltip.Delay);
            
            if (tooltipWidget != null && _currentTarget == target)
            {
                tooltipWidget.SetText(target.Tooltip.TooltipText);
                tooltipWidget.RectTransform.position = Input.mousePosition + (Vector3)target.Tooltip.Offset;
                tooltipWidget.Show();
                // Hier könnten weitere Tooltip-Eigenschaften gesetzt werden (Farbe, etc.)
            }
        }

        public void HideTooltip(UIWidget target, bool immediate = false)
        {
            if (_currentTarget != target && !immediate) return;
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            if (tooltipWidget != null) tooltipWidget.Hide();
            _currentTarget = null;
        }
    }
}