using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivors.Weapons
{
    /// <summary>
    /// Beginner-friendly weapon manager:
    /// - Main weapon (gun) levels 1..5
    /// - Up to 3 equipped special tools/weapons at the same time
    /// - Every weapon max level 5
    /// - Generates 3 random upgrade choices on level-up
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        public const int MaxLevel = 5;
        public const int MaxSpecialWeapons = 3;

        [Header("Main Weapon Reference")]
        public WeaponController mainGun;

        [Header("Runtime Levels")]
        [SerializeField] private int _mainGunLevel = 1;
        private readonly Dictionary<WeaponId, int> _levels = new Dictionary<WeaponId, int>();

        public event Action OnLoadoutChanged;

        public int MainGunLevel => _mainGunLevel;

        public int GetLevel(WeaponId id)
        {
            if (id == WeaponId.MainGun) return _mainGunLevel;
            return _levels.TryGetValue(id, out int lvl) ? lvl : 0;
        }

        public int OwnedSpecialCount => _levels.Count;

        public bool HasWeapon(WeaponId id) => GetLevel(id) > 0;

        private void Awake()
        {
            if (mainGun == null)
                mainGun = GetComponent<WeaponController>();
        }

        private void Start()
        {
            // Auto-bind to UI if present.
            UIManager ui = FindFirstObjectByType<UIManager>();
            ui?.SetWeaponManager(this);
            OnLoadoutChanged?.Invoke();
        }

        public IEnumerable<WeaponId> GetOwnedSpecialWeapons()
        {
            return _levels.Keys;
        }

        public List<WeaponUpgradeChoice> RollChoices(int count = 3)
        {
            List<WeaponUpgradeChoice> pool = BuildPool();
            List<WeaponUpgradeChoice> result = new List<WeaponUpgradeChoice>(count);

            if (pool.Count <= count)
                return pool;

            // Simple beginner-friendly random unique picks.
            while (result.Count < count && pool.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx);
            }

            return result;
        }

        private List<WeaponUpgradeChoice> BuildPool()
        {
            List<WeaponUpgradeChoice> pool = new List<WeaponUpgradeChoice>();

            // Main weapon upgrade can always appear until max level.
            if (_mainGunLevel < MaxLevel)
            {
                pool.Add(new WeaponUpgradeChoice
                {
                    WeaponId = WeaponId.MainGun,
                    CurrentLevel = _mainGunLevel,
                    NextLevel = _mainGunLevel + 1,
                    Title = $"Main Gun Lv{_mainGunLevel} → Lv{_mainGunLevel + 1}",
                    Description = "Higher fire rate and faster projectiles."
                });
            }

            bool canUnlockNewSpecial = OwnedSpecialCount < MaxSpecialWeapons;

            // Special weapons: unlocks only if we still have room.
            if (canUnlockNewSpecial)
            {
                AddUnlockIfMissing(pool, WeaponId.RocketLauncher, "Rocket Launcher", "Unlock: fires rockets that explode (area damage).");
                AddUnlockIfMissing(pool, WeaponId.RotatingSaw, "Rotating Saw", "Unlock: saw blades orbit you and shred enemies while active.");
                AddUnlockIfMissing(pool, WeaponId.SlowingField, "Slowing Field", "Unlock: aura slows enemies inside.");
                AddUnlockIfMissing(pool, WeaponId.Boomerang, "Boomerang", "Unlock: boomerangs fly out and return, hitting twice.");
                AddUnlockIfMissing(pool, WeaponId.FireShoes, "Fire Shoes", "Unlock: leave a burning trail that damages enemies.");
            }

            // Upgrades for owned specials (exclude max level).
            AddUpgradeIfOwned(pool, WeaponId.RocketLauncher, "Rocket Launcher");
            AddUpgradeIfOwned(pool, WeaponId.RotatingSaw, "Rotating Saw");
            AddUpgradeIfOwned(pool, WeaponId.SlowingField, "Slowing Field");
            AddUpgradeIfOwned(pool, WeaponId.Boomerang, "Boomerang");
            AddUpgradeIfOwned(pool, WeaponId.FireShoes, "Fire Shoes");

            return pool;
        }

        private void AddUnlockIfMissing(List<WeaponUpgradeChoice> pool, WeaponId id, string name, string unlockText)
        {
            if (HasWeapon(id)) return;
            pool.Add(new WeaponUpgradeChoice
            {
                WeaponId = id,
                CurrentLevel = 0,
                NextLevel = 1,
                Title = $"{name} (Unlock)",
                Description = unlockText
            });
        }

        private void AddUpgradeIfOwned(List<WeaponUpgradeChoice> pool, WeaponId id, string name)
        {
            int current = GetLevel(id);
            if (current <= 0) return;
            if (current >= MaxLevel) return;

            pool.Add(new WeaponUpgradeChoice
            {
                WeaponId = id,
                CurrentLevel = current,
                NextLevel = current + 1,
                Title = $"{name} Lv{current} → Lv{current + 1}",
                Description = GetUpgradeDescription(id, current + 1)
            });
        }

        public void ApplyChoice(WeaponUpgradeChoice choice)
        {
            if (choice == null) return;

            if (choice.WeaponId == WeaponId.MainGun)
            {
                UpgradeMainGun();
                OnLoadoutChanged?.Invoke();
                return;
            }

            if (choice.IsUnlock)
                UnlockSpecial(choice.WeaponId);
            else
                UpgradeSpecial(choice.WeaponId);

            OnLoadoutChanged?.Invoke();
        }

        private void UpgradeMainGun()
        {
            if (_mainGunLevel >= MaxLevel) return;
            _mainGunLevel++;

            if (mainGun != null)
            {
                // Simple scaling: noticeable but not chaotic.
                mainGun.fireRate += 0.18f;
                mainGun.projectileSpeed += 1.2f;
            }
        }

        private void UnlockSpecial(WeaponId id)
        {
            if (OwnedSpecialCount >= MaxSpecialWeapons) return;
            if (HasWeapon(id)) return;

            _levels[id] = 1;
            ApplySpecialLevelToComponent(id, 1);
        }

        private void UpgradeSpecial(WeaponId id)
        {
            int current = GetLevel(id);
            if (current <= 0) return;
            if (current >= MaxLevel) return;
            int next = current + 1;
            _levels[id] = next;
            ApplySpecialLevelToComponent(id, next);
        }

        private void ApplySpecialLevelToComponent(WeaponId id, int level)
        {
            // Each special weapon has its own component. Keep this switch simple & explicit.
            switch (id)
            {
                case WeaponId.RocketLauncher:
                {
                    var w = GetComponent<VoidSurvivors.Weapons.RocketLauncher.RocketLauncherWeapon>();
                    if (w == null) w = gameObject.AddComponent<VoidSurvivors.Weapons.RocketLauncher.RocketLauncherWeapon>();
                    w.SetLevel(level);
                    break;
                }
                case WeaponId.RotatingSaw:
                {
                    var w = GetComponent<VoidSurvivors.Weapons.RotatingSaw.RotatingSawWeapon>();
                    if (w == null) w = gameObject.AddComponent<VoidSurvivors.Weapons.RotatingSaw.RotatingSawWeapon>();
                    w.SetLevel(level);
                    break;
                }
                case WeaponId.SlowingField:
                {
                    var w = GetComponent<VoidSurvivors.Weapons.SlowingField.SlowingFieldWeapon>();
                    if (w == null) w = gameObject.AddComponent<VoidSurvivors.Weapons.SlowingField.SlowingFieldWeapon>();
                    w.SetLevel(level);
                    break;
                }
                case WeaponId.Boomerang:
                {
                    var w = GetComponent<VoidSurvivors.Weapons.Boomerang.BoomerangWeapon>();
                    if (w == null) w = gameObject.AddComponent<VoidSurvivors.Weapons.Boomerang.BoomerangWeapon>();
                    w.SetLevel(level);
                    break;
                }
                case WeaponId.FireShoes:
                {
                    var w = GetComponent<VoidSurvivors.Weapons.FireShoes.FireShoesWeapon>();
                    if (w == null) w = gameObject.AddComponent<VoidSurvivors.Weapons.FireShoes.FireShoesWeapon>();
                    w.SetLevel(level);
                    break;
                }
                // Others will be wired in as we implement them.
            }
        }

        public static string GetWeaponName(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.MainGun: return "Main Gun";
                case WeaponId.RocketLauncher: return "Rocket Launcher";
                case WeaponId.RotatingSaw: return "Rotating Saw";
                case WeaponId.SlowingField: return "Slowing Field";
                case WeaponId.Boomerang: return "Boomerang";
                case WeaponId.FireShoes: return "Fire Shoes";
                default: return id.ToString();
            }
        }

        private static string GetUpgradeDescription(WeaponId id, int nextLevel)
        {
            // Short descriptions used on the upgrade cards.
            switch (id)
            {
                case WeaponId.RocketLauncher:
                    if (nextLevel >= 1 && nextLevel <= 4) return "Fires +1 additional rocket.";
                    if (nextLevel == 5) return "Stronger explosion (radius + damage).";
                    return "Upgrade rockets.";
                case WeaponId.RotatingSaw:
                    if (nextLevel >= 1 && nextLevel <= 5) return "Adds +1 saw blade.";
                    return "Upgrade saws.";
                case WeaponId.SlowingField:
                    return "Larger radius and stronger slow.";
                case WeaponId.Boomerang:
                    if (nextLevel >= 1 && nextLevel <= 5) return "Adds +1 boomerang (slightly more range).";
                    return "Upgrade boomerangs.";
                case WeaponId.FireShoes:
                    return "More burn damage and longer-lasting trail.";
                default:
                    return "Upgrade.";
            }
        }
    }
}

