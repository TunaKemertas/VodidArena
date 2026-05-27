using UnityEngine;
using VoidSurvivors.Weapons;

public class XPManager : MonoBehaviour
{
    [Header("XP Progression")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 20;
    public float xpGrowth = 1.25f;

    [Header("References")]
    public WeaponController weapon;
    public WeaponManager weaponManager;

    private UIManager _ui;

    private void Start()
    {
        _ui = FindFirstObjectByType<UIManager>();
        if (weapon == null)
            weapon = FindFirstObjectByType<WeaponController>();
        if (weaponManager == null)
            weaponManager = GetComponent<WeaponManager>();

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
        xpToNextLevel = Mathf.CeilToInt(xpToNextLevel * xpGrowth);
        _ui?.SetLevel(level);

        // Level-up flow: pause and present 3 random choices.
        if (weaponManager == null)
            weaponManager = GetComponent<WeaponManager>();

        if (_ui != null && weaponManager != null)
        {
            GameManager.Instance?.EnterLevelUpPause();
            var choices = weaponManager.RollChoices(3);
            if (choices == null || choices.Count == 0)
            {
                GameManager.Instance?.ExitLevelUpPause();
                return;
            }
            _ui.ShowLevelUpChoices(level, choices, picked =>
            {
                weaponManager.ApplyChoice(picked);
                _ui.HideLevelUpChoices();
                GameManager.Instance?.ExitLevelUpPause();
            });
        }
        else
        {
            // Fallback (shouldn't happen): keep old simple juice.
            _ui?.PlayLevelUp(level);
        }
    }
}
