using System;

namespace DragonCeltas
{
    public static class GameEvents
    {
        public static event Action OnEnemyDeath;
        public static event Action<int> OnScoreGained;
        public static event Action<int> OnGoldGained;

        public static void EnemyDied()
        {
            OnEnemyDeath?.Invoke();
        }

        public static void ScoreGained(int points)
        {
            OnScoreGained?.Invoke(points);
        }

        public static void GoldGained(int amount)
        {
            OnGoldGained?.Invoke(amount);
        }
    }
}