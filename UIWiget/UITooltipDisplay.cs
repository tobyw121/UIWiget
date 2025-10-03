using UnityEngine;
using System.Collections;
using YourGame.UI.Widgets;

namespace YourGame.UI
{
    public class UITooltipDisplay : MonoBehaviour
    {
        public static UITooltipDisplay Instance { get; private set; }
        
        [SerializeField] private UIWidget tooltipWidget;
        private Coroutine _showCoroutine;
        private UIWidget _currentTarget;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            if (tooltipWidget != null) tooltipWidget.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            UIWidget.OnGlobalPointerEnter += HandleWidgetPointerEnter;
            UIWidget.OnGlobalPointerExit += HandleWidgetPointerExit;
        }

        private void OnDisable()
        {
            UIWidget.OnGlobalPointerEnter -= HandleWidgetPointerEnter;
            UIWidget.OnGlobalPointerExit -= HandleWidgetPointerExit;
        }

        private void HandleWidgetPointerEnter(UIWidget target)
        {
            ShowTooltip(target);
        }

        private void HandleWidgetPointerExit(UIWidget target)
        {
            HideTooltip(target);
        }

        public void ShowTooltip(UIWidget target)
        {
            string resolvedText = target.GetResolvedTooltipText();

            if (target == null || !target.Tooltip.Enabled || string.IsNullOrEmpty(resolvedText)) return;
            
            _currentTarget = target;
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ShowTooltipRoutine(target, resolvedText));
        }

        private IEnumerator ShowTooltipRoutine(UIWidget target, string textToShow)
        {
            yield return new WaitForSecondsRealtime(target.Tooltip.Delay);
            if (tooltipWidget != null && _currentTarget == target)
            {
                tooltipWidget.SetText(textToShow);
                tooltipWidget.RectTransform.position = Input.mousePosition + (Vector3)target.Tooltip.Offset;
                tooltipWidget.Show();
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