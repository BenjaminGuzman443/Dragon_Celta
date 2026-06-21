using UnityEngine;

namespace DragonCeltas
{
    public class WarriorController : MonoBehaviour
    {
        public static int Score => PlayerUpgrades.Score;
        public static bool IsDead => PlayerHealth.IsDead;

        public static void AddScore(int points) => PlayerUpgrades.AddScore(points);
    }
}