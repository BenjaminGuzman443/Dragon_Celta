using UnityEngine;

namespace DragonCeltas
{
    public interface IEnemy
    {
        void TakeDamage(float damage);
        void Stun(float duration);
        void ApplyKnockback(Vector2 direction, float force);
    }
}
