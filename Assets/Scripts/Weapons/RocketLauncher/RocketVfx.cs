using DG.Tweening;
using UnityEngine;

namespace VoidSurvivors.Weapons.RocketLauncher
{
    public static class RocketVfx
    {
        public static void SpawnExplosion(Vector2 position, float radius)
        {
            float r = Mathf.Clamp(radius, 0.2f, 3.5f);

            GameObject go = new GameObject("RocketExplosionVFX");
            go.transform.position = position;
            AutoSprite2D.AddTo(go, new Color(1f, 0.65f, 0.2f, 0.85f), sortingOrder: 30);
            go.transform.localScale = Vector3.zero;

            // Simple "pop" + fade. Uses the embedded DG.Tweening compatibility layer.
            go.transform.DOScale(new Vector3(r * 2f, r * 2f, 1f), 0.18f).SetEase(Ease.OutBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    CanvasGroup cg = go.AddComponent<CanvasGroup>();
                    cg.alpha = 1f;
                    cg.DOFade(0f, 0.18f).SetEase(Ease.OutQuad).SetUpdate(true)
                        .OnComplete(() => Object.Destroy(go));
                });
        }
    }
}

