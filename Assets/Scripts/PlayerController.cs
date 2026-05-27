using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP = 100;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;

        // Beginner-friendly input (WASD / arrows).
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;

        Vector2 targetPos = _rb.position + _moveInput * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        _ui?.SetHP(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}

