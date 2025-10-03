using System;
using System.Collections.Generic;
using UnityEngine;
using YourGame.UI.Widgets;

public class UITweener : MonoBehaviour
{
    public static UITweener Instance { get; private set; }

    private abstract class Tween
    {
        public float duration;
        public float startTime;
        public Easing.EaseType easeType;
        public Action onComplete;
        public bool IsComplete => Time.unscaledTime >= startTime + duration;
        public abstract void Update();
    }

    private class FloatTween : Tween { public Action<float> setter; public float start, end; public override void Update() => setter(Easing.GetValue(easeType, start, end, (Time.unscaledTime - startTime) / duration)); }
    private class Vector2Tween : Tween { public Action<Vector2> setter; public Vector2 start, end; public override void Update() => setter(Vector2.LerpUnclamped(start, end, Easing.GetValue(easeType, 0, 1, (Time.unscaledTime - startTime) / duration))); }
    private class Vector3Tween : Tween { public Action<Vector3> setter; public Vector3 start, end; public override void Update() => setter(Vector3.LerpUnclamped(start, end, Easing.GetValue(easeType, 0, 1, (Time.unscaledTime - startTime) / duration))); }
    private class ColorTween : Tween { public Action<Color> setter; public Color start, end; public override void Update() => setter(Color.LerpUnclamped(start, end, Easing.GetValue(easeType, 0, 1, (Time.unscaledTime - startTime) / duration))); }

    private readonly List<Tween> _activeTweens = new List<Tween>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            var tween = _activeTweens[i];
            tween.Update();
            if (tween.IsComplete)
            {
                tween.onComplete?.Invoke();
                _activeTweens.RemoveAt(i);
            }
        }
    }
    
    public void AddTween(Action<float> setter, float start, float end, float duration, Easing.EaseType ease, Action onComplete = null) => _activeTweens.Add(new FloatTween { setter = setter, start = start, end = end, duration = duration, easeType = ease, onComplete = onComplete, startTime = Time.unscaledTime });
    public void AddTween(Action<Vector2> setter, Vector2 start, Vector2 end, float duration, Easing.EaseType ease, Action onComplete = null) => _activeTweens.Add(new Vector2Tween { setter = setter, start = start, end = end, duration = duration, easeType = ease, onComplete = onComplete, startTime = Time.unscaledTime });
    public void AddTween(Action<Vector3> setter, Vector3 start, Vector3 end, float duration, Easing.EaseType ease, Action onComplete = null) => _activeTweens.Add(new Vector3Tween { setter = setter, start = start, end = end, duration = duration, easeType = ease, onComplete = onComplete, startTime = Time.unscaledTime });
    public void AddTween(Action<Color> setter, Color start, Color end, float duration, Easing.EaseType ease, Action onComplete = null) => _activeTweens.Add(new ColorTween { setter = setter, start = start, end = end, duration = duration, easeType = ease, onComplete = onComplete, startTime = Time.unscaledTime });
    public void AddDelay(float duration, Action onComplete) => _activeTweens.Add(new FloatTween { setter = (f)=>{}, duration = duration, onComplete = onComplete, startTime = Time.unscaledTime});
}