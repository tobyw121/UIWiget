using System;
using System.Collections;
using UnityEngine;

namespace YourGame.UI.Widgets.Animation
{
    internal static class WidgetAnimationUtility
    {
        public static IEnumerator AnimateFloat(Action<float> setter, float start, float end, float duration, Easing.EaseType ease)
        {
            if (setter == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                setter(end);
                yield break;
            }

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedValue = Easing.GetValue(ease, start, end, progress);
                setter(easedValue);
                yield return null;
            }

            setter(end);
        }

        public static IEnumerator AnimateVector2(Action<Vector2> setter, Vector2 start, Vector2 end, float duration, Easing.EaseType ease)
        {
            if (setter == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                setter(end);
                yield break;
            }

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedProgress = Easing.GetValue(ease, 0f, 1f, progress);
                setter(Vector2.LerpUnclamped(start, end, easedProgress));
                yield return null;
            }

            setter(end);
        }

        public static IEnumerator AnimateVector3(Action<Vector3> setter, Vector3 start, Vector3 end, float duration, Easing.EaseType ease)
        {
            if (setter == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                setter(end);
                yield break;
            }

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedProgress = Easing.GetValue(ease, 0f, 1f, progress);
                setter(Vector3.LerpUnclamped(start, end, easedProgress));
                yield return null;
            }

            setter(end);
        }

        public static IEnumerator AnimateQuaternion(Action<Quaternion> setter, Quaternion start, Quaternion end, float duration, Easing.EaseType ease)
        {
            if (setter == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                setter(end);
                yield break;
            }

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float easedProgress = Easing.GetValue(ease, 0f, 1f, progress);
                setter(Quaternion.SlerpUnclamped(start, end, easedProgress));
                yield return null;
            }

            setter(end);
        }
    }
}
