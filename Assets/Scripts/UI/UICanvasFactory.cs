using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Shared helpers for building mobile-ready UI canvases and widgets at runtime.
/// </summary>
public static class UICanvasFactory
{
    public const float RefWidth = 1080f;
    public const float RefHeight = 1920f;

    public static RectTransform CreateCanvasRoot(string name, out Canvas canvas)
    {
        EnsureEventSystem();

        GameObject canvasGo = new GameObject(name);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
        // Portrait mobile: favor height matching so HUD stays readable on tall phones.
        scaler.matchWidthOrHeight = 0.35f;

        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject safeGo = new GameObject("SafeArea");
        safeGo.transform.SetParent(canvasGo.transform, false);
        RectTransform safe = safeGo.AddComponent<RectTransform>();
        StretchFull(safe);
        safeGo.AddComponent<SafeArea>();

        return safe;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    public static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    public static Image CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        StretchFull(rt);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static Text CreateText(Transform parent, string name, string text, int size, TextAnchor anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = text;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        return t;
    }

    public static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(label.Replace(" ", "") + "Button");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.14f, 0.14f, 0.18f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        Text t = CreateText(go.transform, "Label", label, 36, TextAnchor.MiddleCenter);
        RectTransform trt = t.GetComponent<RectTransform>();
        StretchFull(trt);

        go.AddComponent<UIButtonFeedback>();
        return btn;
    }

    public static Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size, string label, float value)
    {
        GameObject row = new GameObject(name + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.pivot = new Vector2(0.5f, 0.5f);
        rowRt.sizeDelta = new Vector2(size.x, size.y + 40f);
        rowRt.anchoredPosition = pos;

        Text labelText = CreateText(row.transform, "Label", label, 28, TextAnchor.UpperLeft);
        RectTransform lrt = labelText.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0, 1);
        lrt.anchorMax = new Vector2(1, 1);
        lrt.pivot = new Vector2(0, 1);
        lrt.sizeDelta = new Vector2(0, 36);
        lrt.anchoredPosition = Vector2.zero;

        GameObject sliderGo = new GameObject(name);
        sliderGo.transform.SetParent(row.transform, false);
        RectTransform srt = sliderGo.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0);
        srt.anchorMax = new Vector2(1, 0);
        srt.pivot = new Vector2(0.5f, 0);
        srt.sizeDelta = new Vector2(0, size.y);
        srt.anchoredPosition = Vector2.zero;

        Image bg = sliderGo.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        Slider slider = sliderGo.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;

        GameObject fillArea = new GameObject("Fill");
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform frt = fillArea.AddComponent<RectTransform>();
        StretchFull(frt);
        frt.offsetMin = new Vector2(8, 8);
        frt.offsetMax = new Vector2(-8, -8);
        Image fill = fillArea.AddComponent<Image>();
        fill.color = new Color(0.35f, 0.85f, 1f, 1f);
        slider.fillRect = frt;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderGo.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(1, 1, 1, 0.01f);
        RectTransform hrt = handle.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(20, 20);
        slider.handleRect = hrt;
        slider.targetGraphic = handleImg;

        return slider;
    }

    public static Image CreateBar(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color fillColor, out Image fill)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.65f);

        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(go.transform, false);
        RectTransform frt = fillGo.AddComponent<RectTransform>();
        fill = fillGo.AddComponent<Image>();
        fill.color = fillColor;
        fill.raycastTarget = false;
        SetBarFillAmount(frt, 0f);

        return bg;
    }

    /// <summary>
    /// Resizes a bar fill by anchor (works without sprites; Image.Type.Filled does not).
    /// </summary>
    public static void SetBarFillAmount(RectTransform fillRect, float amount)
    {
        if (fillRect == null) return;
        amount = Mathf.Clamp01(amount);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(amount, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
    }
}
