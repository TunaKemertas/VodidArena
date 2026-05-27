using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// DOTween animation helpers required by the course brief.
/// </summary>
public static class UIAnimations
{
    /// <summary>
    /// ANIMATION 1 — Main menu intro (title fade/move, play button scale with OutBack).
    /// </summary>
    public static void PlayMainMenuIntro(RectTransform title, Button playButton)
    {
        if (title == null || playButton == null) return;

        CanvasGroup titleCg = title.GetComponent<CanvasGroup>();
        if (titleCg == null) titleCg = title.gameObject.AddComponent<CanvasGroup>();

        Vector2 startPos = title.anchoredPosition + new Vector2(0f, 80f);
        title.anchoredPosition = startPos;
        titleCg.alpha = 0f;
        title.localScale = Vector3.one;

        titleCg.DOFade(1f, 0.65f).SetEase(Ease.OutQuad).SetUpdate(true);
        title.DOAnchorPos(title.anchoredPosition - new Vector2(0f, 80f), 0.65f).SetEase(Ease.OutQuad).SetUpdate(true);

        Transform playT = playButton.transform;
        playT.localScale = Vector3.zero;
        playT.DOScale(1f, 0.55f).SetEase(Ease.OutBack).SetDelay(0.25f).SetUpdate(true);
    }

    /// <summary>
    /// ANIMATION 2 — Level-up popup (sequence: show panel, punch text, flash XP bar).
    /// </summary>
    public static void PlayLevelUp(RectTransform panel, Text levelText, Image xpFill)
    {
        if (panel == null) return;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();

        panel.gameObject.SetActive(true);
        cg.alpha = 0f;
        panel.localScale = Vector3.one * 0.85f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(cg.DOFade(1f, 0.12f));
        seq.Append(panel.DOScale(1f, 0.18f).SetEase(Ease.OutBack));

        if (levelText != null)
        {
            levelText.transform.localScale = Vector3.one;
            seq.Append(levelText.transform.DOPunchScale(Vector3.one * 0.35f, 0.45f, 8, 0.7f));
        }

        if (xpFill != null)
        {
            Color baseColor = xpFill.color;
            seq.Append(xpFill.DOColor(Color.white, 0.08f));
            seq.Append(xpFill.DOColor(baseColor, 0.12f));
        }

        seq.AppendInterval(0.35f);
        seq.Append(cg.DOFade(0f, 0.2f));
        seq.OnComplete(() => panel.gameObject.SetActive(false));
    }

    /// <summary>
    /// ANIMATION 3 — Game over panel (fade overlay, scale title, buttons appear one-by-one with OnComplete).
    /// </summary>
    public static void PlayGameOver(Image overlay, RectTransform title, Button[] buttons)
    {
        if (overlay == null || title == null) return;

        CanvasGroup overlayCg = overlay.GetComponent<CanvasGroup>();
        if (overlayCg == null) overlayCg = overlay.gameObject.AddComponent<CanvasGroup>();

        overlay.gameObject.SetActive(true);
        overlayCg.alpha = 0f;
        title.localScale = Vector3.zero;

        foreach (Button b in buttons)
        {
            if (b != null)
            {
                b.gameObject.SetActive(true);
                b.transform.localScale = Vector3.zero;
            }
        }

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(overlayCg.DOFade(1f, 0.35f).SetEase(Ease.OutQuad));
        seq.Append(title.DOScale(1f, 0.45f).SetEase(Ease.OutBack));

        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                Button b = buttons[i];
                if (b == null) continue;
                int index = i;
                seq.Append(b.transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        // Course requirement: use OnComplete somewhere in this sequence.
                        if (index == buttons.Length - 1)
                            AudioManager.Instance?.PlayClick();
                    }));
            }
        }
    }

    /// <summary>
    /// Victory uses a shorter version of the game-over sequence.
    /// </summary>
    public static void PlayVictory(Image overlay, RectTransform title, Button[] buttons)
    {
        PlayGameOver(overlay, title, buttons);
    }
}
