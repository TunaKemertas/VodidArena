using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 18f;
    public float deceleration = 22f;

    [Header("Health")]
    public int maxHP = 55;
    public int currentHP = 55;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Vector2 _smoothedMoveInput;
    private UIManager _ui;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        currentHP = Mathf.Clamp(currentHP, 1, maxHP);
    }

    private void Start()
    {
        _ui = FindFirstObjectByType<UIManager>();
        _ui?.SetHP(currentHP, maxHP);
    }

    private void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
            return;

        Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 joystick = MobileJoystickInput.Instance != null ? MobileJoystickInput.Instance.Direction : Vector2.zero;

        // Joystick drives mobile movement; keyboard is kept for PC/editor testing.
        Vector2 targetInput = joystick.sqrMagnitude > 0.01f ? joystick : keyboard;
        targetInput = Vector2.ClampMagnitude(targetInput, 1f);

        float rate = targetInput.sqrMagnitude > _smoothedMoveInput.sqrMagnitude ? acceleration : deceleration;
        _smoothedMoveInput = Vector2.MoveTowards(_smoothedMoveInput, targetInput, rate * Time.deltaTime);
        _moveInput = Vector2.ClampMagnitude(_smoothedMoveInput, 1f);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
            return;

        Vector2 targetPos = _rb.position + _moveInput * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        _ui?.SetHP(currentHP, maxHP);

        // Juice: small camera shake + hit sound when the player is damaged.
        CameraFollow2D cam = Camera.main != null ? Camera.main.GetComponent<CameraFollow2D>() : null;
        cam?.Shake(0.15f, 0.18f);
        AudioManager.Instance?.PlayHit();

        if (currentHP <= 0)
            Die();
    }

    public void HealPercent(float percent)
    {
        if (percent <= 0f) return;
        if (currentHP <= 0) return;

        int heal = Mathf.CeilToInt(maxHP * Mathf.Clamp01(percent));
        currentHP = Mathf.Min(maxHP, currentHP + heal);
        _ui?.SetHP(currentHP, maxHP);
    }

    private void Die()
    {
        GameManager.Instance?.GameOver();
    }
}
