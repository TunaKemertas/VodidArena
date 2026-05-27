using System;
using UnityEngine;
using UnityEngine.UI;

namespace DG.Tweening
{
    public static class TweenExtensions
    {
        public static void DOKill(this Transform target)
        {
            TweenRunner.KillTarget(target);
        }

        public static void DOKill(this CanvasGroup target)
        {
            TweenRunner.KillTarget(target);
        }

        public static TweenBase DOScale(this Transform target, float endValue, float duration)
        {
            return new Vector3Tween(
                target,
                target.localScale,
                Vector3.one * endValue,
                v => target.localScale = v,
                duration);
        }

        public static TweenBase DOScale(this Transform target, Vector3 endValue, float duration)
        {
            return new Vector3Tween(
                target,
                target.localScale,
                endValue,
                v => target.localScale = v,
                duration);
        }

        public static TweenBase DOPunchScale(this Transform target, Vector3 punch, float duration, int vibrato, float elasticity)
        {
            return new PunchScaleTween(target, punch, duration, elasticity);
        }

        public static TweenBase DOAnchorPos(this RectTransform target, Vector2 endValue, float duration)
        {
            return new Vector2Tween(
                target,
                target.anchoredPosition,
                endValue,
                v => target.anchoredPosition = v,
                duration);
        }

        public static TweenBase DOFade(this CanvasGroup target, float endValue, float duration)
        {
            return new FloatTween(
                target,
                () => target.alpha,
                v => target.alpha = v,
                endValue,
                duration);
        }

        public static TweenBase DOColor(this Graphic target, Color endValue, float duration)
        {
            return new ColorTween(
                target,
                target.color,
                endValue,
                v => target.color = v,
                duration);
        }
    }
}
