using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AutoSprite2D : MonoBehaviour
{
    public Color color = Color.white;
    public int pixels = 16;

    private static Material _unlitMaterial;
    private static Texture2D _whiteTexture;
    private SpriteRenderer _sr;

    public static AutoSprite2D AddTo(GameObject go, Color tint, int sortingOrder, int pixelSize = 16)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;

        AutoSprite2D auto = go.GetComponent<AutoSprite2D>();
        if (auto == null) auto = go.AddComponent<AutoSprite2D>();

        auto.color = tint;
        auto.pixels = pixelSize;
        auto.Build();
        return auto;
    }

    private void Start()
    {
        Build();
    }

    public void Build()
    {
        _sr = GetComponent<SpriteRenderer>();
        EnsureSharedAssets();

        int size = Mathf.Clamp(pixels, 4, 64);
        _sr.sprite = Sprite.Create(_whiteTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        _sr.color = color;
        if (_unlitMaterial != null) _sr.sharedMaterial = _unlitMaterial;
    }

    private static void EnsureSharedAssets()
    {
        if (_whiteTexture == null)
        {
            _whiteTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            _whiteTexture.filterMode = FilterMode.Point;
            Color[] px = new Color[256];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            _whiteTexture.SetPixels(px);
            _whiteTexture.Apply(false, false);
        }

        if (_unlitMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null) _unlitMaterial = new Material(shader);
        }
    }
}
