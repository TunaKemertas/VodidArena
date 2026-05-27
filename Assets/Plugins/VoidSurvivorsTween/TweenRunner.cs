using System;
using System.Collections.Generic;
using UnityEngine;

namespace DG.Tweening
{
    public class TweenRunner : MonoBehaviour
    {
        private static TweenRunner _instance;
        private readonly List<TweenBase> _active = new List<TweenBase>();

        public static TweenRunner Instance
        {
            get
            {
                if (_instance != null) return _instance;
                GameObject go = new GameObject("TweenRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<TweenRunner>();
                return _instance;
            }
        }

        public static void Register(TweenBase tween)
        {
            Instance._active.Add(tween);
        }

        public static void Unregister(TweenBase tween)
        {
            if (_instance == null) return;
            _instance._active.Remove(tween);
        }

        public static void KillTarget(object target)
        {
            if (_instance == null) return;
            for (int i = _instance._active.Count - 1; i >= 0; i--)
            {
                if (_instance._active[i].Target == target)
                    _instance._active[i].Kill();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float unscaledDt = Time.unscaledDeltaTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                TweenBase t = _active[i];
                if (t == null || t.IsComplete)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                // Unity "fake null" when the tween target was destroyed (scene reload, UI rebuild, etc.).
                if (t.Target is UnityEngine.Object unityTarget && unityTarget == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                t.Tick(t.UseUnscaledTime ? unscaledDt : dt);
                if (t.IsComplete)
                    _active.RemoveAt(i);
            }
        }
    }

    public abstract class TweenBase
    {
        public object Target { get; protected set; }
        public bool UseUnscaledTime { get; protected set; }
        public bool IsComplete { get; protected set; }

        protected float Duration;
        protected float Delay;
        protected Ease EaseType = Ease.Linear;
        protected Action OnCompleteCallback;
        protected float _elapsed;

        public TweenBase SetEase(Ease ease)
        {
            EaseType = ease;
            return this;
        }

        public TweenBase SetDelay(float delay)
        {
            Delay = Mathf.Max(0f, delay);
            return this;
        }

        public TweenBase SetUpdate(bool useUnscaledTime)
        {
            UseUnscaledTime = useUnscaledTime;
            return this;
        }

        /// <summary>Lets <see cref="Sequence"/> propagate unscaled-time to appended tweens (protected setter is not usable on other instances).</summary>
        internal void SetUseUnscaledTime(bool useUnscaledTime)
        {
            UseUnscaledTime = useUnscaledTime;
        }

        public TweenBase OnComplete(Action callback)
        {
            OnCompleteCallback = callback;
            return this;
        }

        public void Kill()
        {
            IsComplete = true;
        }

        public virtual void Tick(float deltaTime)
        {
            if (IsComplete) return;

            _elapsed += deltaTime;
            if (_elapsed < Delay) return;

            float localTime = _elapsed - Delay;
            float t = Duration <= 0f ? 1f : Mathf.Clamp01(localTime / Duration);
            float eased = EaseUtility.Evaluate(EaseType, t);
            Apply(eased, t);

            if (t >= 1f)
            {
                IsComplete = true;
                OnCompleteCallback?.Invoke();
            }
        }

        protected abstract void Apply(float eased, float rawT);
    }

    public class FloatTween : TweenBase
    {
        private readonly Func<float> _getter;
        private readonly Action<float> _setter;
        private readonly float _start;
        private readonly float _end;

        public FloatTween(object target, Func<float> getter, Action<float> setter, float endValue, float duration, bool autoRegister = true)
        {
            Target = target;
            _getter = getter;
            _setter = setter;
            _start = getter();
            _end = endValue;
            Duration = duration;
            if (autoRegister) TweenRunner.Register(this);
        }

        protected override void Apply(float eased, float rawT)
        {
            _setter(Mathf.LerpUnclamped(_start, _end, eased));
        }
    }

    public class Vector3Tween : TweenBase
    {
        private readonly Action<Vector3> _setter;
        private readonly Vector3 _start;
        private readonly Vector3 _end;

        public Vector3Tween(object target, Vector3 start, Vector3 end, Action<Vector3> setter, float duration, bool autoRegister = true)
        {
            Target = target;
            _start = start;
            _end = end;
            _setter = setter;
            Duration = duration;
            if (autoRegister) TweenRunner.Register(this);
        }

        protected override void Apply(float eased, float rawT)
        {
            _setter(Vector3.LerpUnclamped(_start, _end, eased));
        }
    }

    public class Vector2Tween : TweenBase
    {
        private readonly Action<Vector2> _setter;
        private readonly Vector2 _start;
        private readonly Vector2 _end;

        public Vector2Tween(object target, Vector2 start, Vector2 end, Action<Vector2> setter, float duration, bool autoRegister = true)
        {
            Target = target;
            _start = start;
            _end = end;
            _setter = setter;
            Duration = duration;
            if (autoRegister) TweenRunner.Register(this);
        }

        protected override void Apply(float eased, float rawT)
        {
            _setter(Vector2.LerpUnclamped(_start, _end, eased));
        }
    }

    public class ColorTween : TweenBase
    {
        private readonly Action<Color> _setter;
        private readonly Color _start;
        private readonly Color _end;

        public ColorTween(object target, Color start, Color end, Action<Color> setter, float duration, bool autoRegister = true)
        {
            Target = target;
            _start = start;
            _end = end;
            _setter = setter;
            Duration = duration;
            if (autoRegister) TweenRunner.Register(this);
        }

        protected override void Apply(float eased, float rawT)
        {
            _setter(Color.LerpUnclamped(_start, _end, eased));
        }
    }
}
