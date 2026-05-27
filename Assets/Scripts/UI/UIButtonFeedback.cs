using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Small hover/click feedback for UI buttons.
/// </summary>
public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_baseScale * 1.06f, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_baseScale, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayClick();
        transform.DOKill();
        transform.localScale = _baseScale;
        transform.DOPunchScale(Vector3.one * 0.08f, 0.18f, 8, 0.8f).SetUpdate(true);
    }
}
