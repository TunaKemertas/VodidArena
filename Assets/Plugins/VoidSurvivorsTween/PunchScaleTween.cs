using UnityEngine;

namespace DG.Tweening
{
    /// <summary>
    /// Punch scale tween used for level-up and button click feedback.
    /// </summary>
    public class PunchScaleTween : TweenBase
    {
        private readonly Transform _transform;
        private readonly Vector3 _baseScale;
        private readonly Vector3 _punch;
        private readonly float _elasticity;

        public PunchScaleTween(Transform transform, Vector3 punch, float duration, float elasticity)
        {
            Target = transform;
            _transform = transform;
            _baseScale = transform.localScale;
            _punch = punch;
            Duration = duration;
            _elasticity = elasticity;
            TweenRunner.Register(this);
        }

        protected override void Apply(float eased, float rawT)
        {
            float damped = (1f - rawT) * _elasticity;
            Vector3 offset = _punch * Mathf.Sin(rawT * Mathf.PI * 2f) * damped;
            _transform.localScale = _baseScale + offset;

            if (rawT >= 1f)
                _transform.localScale = _baseScale;
        }
    }
}
