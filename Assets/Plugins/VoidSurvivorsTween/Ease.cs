using System;
using UnityEngine;

namespace DG.Tweening
{
    /// <summary>
    /// Lightweight tween easing (subset used by this project).
    /// Same names as DOTween so UI animation code reads identically.
    /// </summary>
    public enum Ease
    {
        Linear,
        OutQuad,
        OutBack
    }

    public static class EaseUtility
    {
        public static float Evaluate(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case Ease.OutQuad:
                    return 1f - (1f - t) * (1f - t);
                case Ease.OutBack:
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                default:
                    return t;
            }
        }
    }
}
