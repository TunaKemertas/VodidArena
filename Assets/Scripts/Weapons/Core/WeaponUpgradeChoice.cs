namespace VoidSurvivors.Weapons
{
    public class WeaponUpgradeChoice
    {
        public WeaponId WeaponId;
        public int CurrentLevel;
        public int NextLevel;
        public string Title;
        public string Description;

        public bool IsUnlock => CurrentLevel <= 0 && NextLevel == 1;
    }
}

