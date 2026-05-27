using UnityEngine;

/// <summary>
/// Mobile joystick host for gameplay HUD.
/// Uses Joystick Pack's Fixed Joystick prefab from Resources/MobileControls/FixedJoystick.
/// </summary>
public class MobileJoystickInput : MonoBehaviour
{
    public static MobileJoystickInput Instance { get; private set; }

    private const string PrefabResourcePath = "MobileControls/FixedJoystick";

    private FixedJoystick _joystick;
    private RectTransform _root;
    private CanvasGroup _canvasGroup;

    public Vector2 Direction { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        UpdateGameplayVisibility();

        if (_joystick == null)
        {
            Direction = Vector2.zero;
            return;
        }

        if (GameManager.Instance != null &&
            (GameManager.Instance.IsPaused || GameManager.Instance.IsLevelUp))
        {
            Direction = Vector2.zero;
            return;
        }

        Direction = Vector2.ClampMagnitude(new Vector2(_joystick.Horizontal, _joystick.Vertical), 1f);
    }

    private void UpdateGameplayVisibility()
    {
        if (_root == null) return;

        // Stay visible during level-up / pause; only hide on end screens.
        bool show = GameManager.Instance == null || !GameManager.Instance.IsGameOverOrWon;
        if (_root.gameObject.activeSelf != show)
            _root.gameObject.SetActive(show);

        if (_canvasGroup == null) return;

        bool blockInput = GameManager.Instance != null &&
                          (GameManager.Instance.IsPaused || GameManager.Instance.IsLevelUp);
        _canvasGroup.blocksRaycasts = !blockInput;
        _canvasGroup.interactable = !blockInput;
        _canvasGroup.alpha = blockInput ? 0.45f : 0.85f;
    }

    public static MobileJoystickInput Create(Transform safeArea)
    {
        GameObject host = new GameObject("MobileJoystickInput");
        host.transform.SetParent(safeArea, false);

        RectTransform rt = host.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(48f, 48f);
        rt.sizeDelta = new Vector2(260f, 260f);

        MobileJoystickInput input = host.AddComponent<MobileJoystickInput>();
        input._root = rt;
        input._canvasGroup = host.AddComponent<CanvasGroup>();
        input._canvasGroup.alpha = 0.85f;
        input._canvasGroup.blocksRaycasts = true;
        input._canvasGroup.interactable = true;

        input.InstantiateJoystickPrefab(host.transform);
        return input;
    }

    private void InstantiateJoystickPrefab(Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(PrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogError("Fixed Joystick prefab missing at Assets/Resources/MobileControls/FixedJoystick.prefab");
            return;
        }

        GameObject joystick = Instantiate(prefab, parent, false);
        joystick.name = "FixedJoystick";

        RectTransform rt = joystick.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Joystick Pack computes drag radius from sizeDelta, so do NOT stretch this RectTransform.
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(130f, 130f);
            rt.sizeDelta = new Vector2(240f, 240f);
            rt.localScale = Vector3.one;
        }

        _joystick = joystick.GetComponentInChildren<FixedJoystick>(true);
        if (_joystick == null)
            Debug.LogError("FixedJoystick component not found on FixedJoystick prefab.");
    }
}
