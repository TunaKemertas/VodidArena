using UnityEngine;

public class XPManager : MonoBehaviour
{
    [Header("XP Progression")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 20;
    [Tooltip("How much the required XP increases each level (multiplier).")]
    public float xpGrowth = 1.25f;

    [Header("References")]
    public WeaponController weapon;

    private UIManager _ui;

    private void Start()
    {
        _ui = FindFirstObjectByType<UIManager>();
        if (weapon == null)
            weapon = FindFirstObjectByType<WeaponController>();

        _ui?.SetLevel(level);
        _ui?.SetXP(currentXP, xpToNextLevel);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        _ui?.SetXP(currentXP, xpToNextLevel);
    }

    private void LevelUp()
    {
        level += 1;

        // Increase the requirement slightly each level.
        xpToNextLevel = Mathf.CeilToInt(xpToNextLevel * xpGrowth);

        // Auto-upgrade the single weapon.
        if (weapon != null)
            weapon.ApplyLevelUpBonus();

        _ui?.SetLevel(level);
    }
}

