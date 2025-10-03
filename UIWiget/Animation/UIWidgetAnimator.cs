using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourGame.UI.Widgets.Animation
{
    internal interface IWidgetTransitionStep
    {
        void PrepareEnter(UIWidget widget);
        void PrepareExit(UIWidget widget);
        IEnumerator PlayEnter(UIWidget widget, float duration, Easing.EaseType ease);
        IEnumerator PlayExit(UIWidget widget, float duration, Easing.EaseType ease);
    }

    internal sealed class UIWidgetAnimator
    {
        private readonly List<IWidgetTransitionStep> _steps = new List<IWidgetTransitionStep>();

        public void Configure(UIWidget widget, UIWidget.VisualTransition transitions, UIWidget.SlideTransition slideConfig)
        {
            _steps.Clear();
            if ((transitions & UIWidget.VisualTransition.Fade) != 0)
            {
                _steps.Add(new FadeTransitionStep());
            }
            if ((transitions & UIWidget.VisualTransition.Scale) != 0)
            {
                _steps.Add(new ScaleTransitionStep());
            }
            if ((transitions & UIWidget.VisualTransition.Slide) != 0)
            {
                _steps.Add(new SlideTransitionStep(slideConfig));
            }
        }

        public IEnumerator PlayShow(UIWidget widget, float duration, Easing.EaseType ease)
        {
            if (_steps.Count == 0)
            {
                yield break;
            }

            foreach (var step in _steps)
            {
                step.PrepareEnter(widget);
            }

            yield return RunSteps(step => step.PlayEnter(widget, duration, ease));
        }

        public IEnumerator PlayHide(UIWidget widget, float duration, Easing.EaseType ease)
        {
            if (_steps.Count == 0)
            {
                yield break;
            }

            foreach (var step in _steps)
            {
                step.PrepareExit(widget);
            }

            yield return RunSteps(step => step.PlayExit(widget, duration, ease));
        }

        private IEnumerator RunSteps(System.Func<IWidgetTransitionStep, IEnumerator> selector)
        {
            var running = new List<IEnumerator>();
            foreach (var step in _steps)
            {
                var enumerator = selector(step);
                if (enumerator != null)
                {
                    running.Add(enumerator);
                }
            }

            if (running.Count == 0)
            {
                yield break;
            }

            while (running.Count > 0)
            {
                for (int i = running.Count - 1; i >= 0; i--)
                {
                    if (!running[i].MoveNext())
                    {
                        running.RemoveAt(i);
                        continue;
                    }
                }
                if (running.Count > 0)
                {
                    yield return null;
                }
            }
        }

        private sealed class FadeTransitionStep : IWidgetTransitionStep
        {
            public void PrepareEnter(UIWidget widget)
            {
                if (widget.CanvasGroup != null)
                {
                    widget.CanvasGroup.alpha = 0f;
                }
            }

            public void PrepareExit(UIWidget widget)
            {
                // no-op, uses current alpha as start value
            }

            public IEnumerator PlayEnter(UIWidget widget, float duration, Easing.EaseType ease)
            {
                var canvasGroup = widget.CanvasGroup;
                if (canvasGroup == null)
                {
                    yield break;
                }

                yield return WidgetAnimationUtility.AnimateFloat(alpha => canvasGroup.alpha = alpha, canvasGroup.alpha, 1f, duration, ease);
            }

            public IEnumerator PlayExit(UIWidget widget, float duration, Easing.EaseType ease)
            {
                var canvasGroup = widget.CanvasGroup;
                if (canvasGroup == null)
                {
                    yield break;
                }

                yield return WidgetAnimationUtility.AnimateFloat(alpha => canvasGroup.alpha = alpha, canvasGroup.alpha, 0f, duration, ease);
            }
        }

        private sealed class ScaleTransitionStep : IWidgetTransitionStep
        {
            public void PrepareEnter(UIWidget widget)
            {
                widget.RectTransform.localScale = Vector3.zero;
            }

            public void PrepareExit(UIWidget widget)
            {
                // keep whatever the widget currently has as start
            }

            public IEnumerator PlayEnter(UIWidget widget, float duration, Easing.EaseType ease)
            {
                yield return WidgetAnimationUtility.AnimateVector3(scale => widget.RectTransform.localScale = scale,
                    widget.RectTransform.localScale,
                    widget.OriginalScale,
                    duration,
                    ease);
            }

            public IEnumerator PlayExit(UIWidget widget, float duration, Easing.EaseType ease)
            {
                yield return WidgetAnimationUtility.AnimateVector3(scale => widget.RectTransform.localScale = scale,
                    widget.RectTransform.localScale,
                    Vector3.zero,
                    duration,
                    ease);
            }
        }

        private sealed class SlideTransitionStep : IWidgetTransitionStep
        {
            private readonly UIWidget.SlideTransition _config;

            public SlideTransitionStep(UIWidget.SlideTransition config)
            {
                _config = config ?? new UIWidget.SlideTransition();
            }

            public void PrepareEnter(UIWidget widget)
            {
                widget.RectTransform.anchoredPosition = widget.OriginalAnchoredPosition + _config.startOffset;
            }

            public void PrepareExit(UIWidget widget)
            {
                // nothing to do before exit animation
            }

            public IEnumerator PlayEnter(UIWidget widget, float duration, Easing.EaseType ease)
            {
                yield return WidgetAnimationUtility.AnimateVector2(position => widget.RectTransform.anchoredPosition = position,
                    widget.RectTransform.anchoredPosition,
                    widget.OriginalAnchoredPosition + _config.endOffset,
                    duration,
                    ease);
            }

            public IEnumerator PlayExit(UIWidget widget, float duration, Easing.EaseType ease)
            {
                yield return WidgetAnimationUtility.AnimateVector2(position => widget.RectTransform.anchoredPosition = position,
                    widget.RectTransform.anchoredPosition,
                    widget.OriginalAnchoredPosition + _config.startOffset,
                    duration,
                    ease);
            }
        }
    }
}
