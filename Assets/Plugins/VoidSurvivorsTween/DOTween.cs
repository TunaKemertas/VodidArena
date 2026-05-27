using System;
using System.Collections.Generic;
using UnityEngine;

namespace DG.Tweening
{
    public class Sequence : TweenBase
    {
        private readonly List<TweenBase> _steps = new List<TweenBase>();
        private int _index;
        private TweenBase _current;
        private float _intervalRemaining;

        private Sequence()
        {
            TweenRunner.Register(this);
        }

        public static Sequence Create()
        {
            return new Sequence();
        }

        public Sequence Append(TweenBase tween)
        {
            TweenRunner.Unregister(tween);
            _steps.Add(tween);
            return this;
        }

        public Sequence AppendInterval(float interval)
        {
            _steps.Add(new IntervalStep(Mathf.Max(0f, interval)));
            return this;
        }

        public new Sequence SetUpdate(bool useUnscaledTime)
        {
            UseUnscaledTime = useUnscaledTime;
            return this;
        }

        public new Sequence OnComplete(Action callback)
        {
            OnCompleteCallback = callback;
            return this;
        }

        public override void Tick(float deltaTime)
        {
            if (IsComplete) return;

            _elapsed += deltaTime;
            if (_elapsed < Delay) return;

            if (_intervalRemaining > 0f)
            {
                _intervalRemaining -= deltaTime;
                return;
            }

            if (_current == null)
            {
                if (_index >= _steps.Count)
                {
                    IsComplete = true;
                    OnCompleteCallback?.Invoke();
                    return;
                }

                _current = _steps[_index++];
                _current.SetUseUnscaledTime(UseUnscaledTime);

                if (_current is IntervalStep interval)
                {
                    _intervalRemaining = interval.Seconds;
                    _current = null;
                    return;
                }
            }

            _current.Tick(deltaTime);
            if (_current.IsComplete)
                _current = null;
        }

        protected override void Apply(float eased, float rawT) { }

        private class IntervalStep : TweenBase
        {
            public float Seconds;
            public IntervalStep(float seconds)
            {
                Seconds = seconds;
                IsComplete = true;
            }
            protected override void Apply(float eased, float rawT) { }
        }
    }

    public static class DOTween
    {
        public static Sequence Sequence()
        {
            return global::DG.Tweening.Sequence.Create();
        }
    }
}
