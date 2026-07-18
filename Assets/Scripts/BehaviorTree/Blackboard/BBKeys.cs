using UnityEngine;

namespace BehaviorTree
{
    public static class BBKeys
    {
        // Movement
        public static readonly BBKey<Vector3> MoveTarget = new("MoveTarget");
        public static readonly BBKey<Vector3> LookTarget = new("LookTarget");

        // Combat
        public static readonly BBKey<DefineThing> ThreatTarget = new("ThreatTarget");
        public static readonly BBKey<float> HealthPercent = new("HealthPercent");
        public static readonly BBKey<bool> HasWeapon = new("HasWeapon");
        public static readonly BBKey<bool> InCombat = new("InCombat");
        public static readonly BBKey<Vector3> CoverPosition = new("CoverPosition");

        // Perception
        public static readonly BBKey<bool> CanSeeEnemy = new("CanSeeEnemy");
        public static readonly BBKey<bool> HeardNoise = new("HeardNoise");
        public static readonly BBKey<Vector3> LastKnownEnemyPosition = new("LastKnownEnemyPosition");
        public static readonly BBKey<Vector3> NoisePosition = new("NoisePosition");

        // Tasks
        public static readonly BBKey<DefineThing> InteractTarget = new("InteractTarget");
        public static readonly BBKey<bool> IsHungry = new("IsHungry");
        public static readonly BBKey<bool> IsThirsty = new("IsThirsty");
        public static readonly BBKey<bool> HasHealingItem = new("HasHealingItem");
        public static readonly BBKey<DefineThing> FoodTarget = new("FoodTarget");

        // Social
        public static readonly BBKey<DefineThing> SocialTarget = new("SocialTarget");
        public static readonly BBKey<int> FactionId = new("FactionId");
    }
}
