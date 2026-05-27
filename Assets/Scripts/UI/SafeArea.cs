using UnityEngine;

/// <summary>
/// Applies mobile safe-area insets (notches) to a full-screen RectTransform.
/// Attach to the root UI container under the Canvas.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform _rt;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea)
            Apply();
    }

    private void Apply()
    {
        _lastSafeArea = Screen.safeArea;

        Vector2 min = _lastSafeArea.position;
        Vector2 max = _lastSafeArea.position + _lastSafeArea.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        _rt.anchorMin = min;
        _rt.anchorMax = max;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }
}
