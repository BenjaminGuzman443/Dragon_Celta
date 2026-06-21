using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragonCeltas
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Ataque")]
        [SerializeField] private float attackDuration = 0.15f;
        [SerializeField] private float attackDelay = 0.25f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackRadius = 1.2f;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Escudo")]
        [SerializeField] private float shieldRadius = 2f;
        [SerializeField] private float shieldPushForce = 10f;
        [SerializeField] private float shieldStunnDuration = 3f;
        [SerializeField] private float shieldCooldown = 10f;

        public bool IsAttacking => estaAtacando;
        public bool IsGuarding => estaEnGuardia;
        public float AttackRadius => attackRadius;
        public float AttackDamage => attackDamage;
        public bool ShieldEnCooldown => shieldCooldownTimer > 0f;
        public float ShieldCooldownNormalized => Mathf.Clamp01(shieldCooldownTimer / shieldCooldown);
        public bool AttackEnCooldown => attackCooldownTimer > 0f;
        public float AttackCooldownNormalized => Mathf.Clamp01(attackCooldownTimer / attackCooldown);

        private float multiplicadorDano = 1f;
        private float danoBasePlano;
        private float bonusRangoAtaque;
        private float bonusVelocidadAtaque;
        private float reduccionCooldownEscudo;
        public bool penalizacionCero;
        public bool velocistaActivo;
        public float vampiricoCuracion;
        public float vampiricoPorcentaje;
        public float bonusXp;

        private Animator animator;
        private PlayerHealth health;
        private PlayerMovement movement;

        private bool estaEnGuardia;
        private bool estaAtacando;
        private float attackTimer;
        private float attackDamageTimer;
        private bool damageApplied;

        private float shieldCooldownTimer;
        private float attackCooldownTimer;
        private bool shieldOnCooldown => shieldCooldownTimer > 0f;

        void Start()
        {
            animator = GetComponent<Animator>();
            health = GetComponent<PlayerHealth>();
            movement = GetComponent<PlayerMovement>();
            danoBasePlano = attackDamage;
        }

        void Update()
        {
            if (PlayerHealth.IsDead) return;

            LeerDefensa();
            LeerAtaque();
            ActualizarAtaque();
            ActualizarCooldownEscudo();
            if (attackCooldownTimer > 0f)
                attackCooldownTimer -= Time.deltaTime;
        }

        private void LeerDefensa()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.wasPressedThisFrame && !shieldOnCooldown && !estaAtacando)
            {
                estaEnGuardia = true;
                ActivarEscudo();
            }

            if (mouse.rightButton.wasReleasedThisFrame)
                estaEnGuardia = false;

            animator.SetBool("EnGuardia", estaEnGuardia);
        }

        private void ActivarEscudo()
        {
            shieldCooldownTimer = GetShieldCooldown();

            MostrarIndicadorEscudo(transform.position, shieldRadius);

            var hits = Physics2D.OverlapCircleAll(transform.position, shieldRadius, enemyLayer);
            var stunned = new HashSet<IEnemy>();
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<IEnemy>();
                if (enemy != null && stunned.Add(enemy))
                {
                    Vector2 dir = ((Vector2)(hit.transform.position - transform.position)).normalized;
                    enemy.ApplyKnockback(dir, shieldPushForce);
                    enemy.Stun(shieldStunnDuration);
                }
            }
        }

        private void MostrarIndicadorEscudo(Vector3 center, float radius)
        {
            var indicator = new GameObject("ShieldIndicator");
            indicator.transform.position = center;

            var sr = indicator.AddComponent<SpriteRenderer>();
            sr.sprite = CrearCirculoLleno(radius);
            sr.color = new Color(1f, 0f, 0f, 0.5f);
            sr.sortingOrder = 100;

            Destroy(indicator, 0.35f);
        }

        private Sprite CrearCirculoLleno(float radius)
        {
            int size = 64;
            var tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float pixelRadius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    Color c = dist <= pixelRadius ? Color.white : new Color(0, 0, 0, 0);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / radius);
        }

        private void ActualizarCooldownEscudo()
        {
            if (shieldCooldownTimer > 0f)
                shieldCooldownTimer -= Time.deltaTime;
        }

        private void LeerAtaque()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (!mouse.leftButton.wasPressedThisFrame) return;
            if (estaEnGuardia || estaAtacando || attackCooldownTimer > 0f) return;

            animator.SetTrigger("Ataque1");
            estaAtacando = true;
            attackTimer = attackDuration;
            attackDamageTimer = GetAttackDelay();
            attackCooldownTimer = GetAttackCooldown();
            damageApplied = false;
        }

        private void ActualizarAtaque()
        {
            if (!estaAtacando) return;

            attackTimer -= Time.deltaTime;

            if (!damageApplied)
            {
                attackDamageTimer -= Time.deltaTime;
                if (attackDamageTimer <= 0f)
                {
                    damageApplied = true;
                    var radio = GetAttackRadius();
                    MostrarIndicadorAtaque(transform.position, radio);
                    var hits = Physics2D.OverlapCircleAll(transform.position, radio, enemyLayer);
                    var damaged = new HashSet<IEnemy>();
                    foreach (var hit in hits)
                    {
                        var enemy = hit.GetComponent<IEnemy>();
                        if (enemy != null && damaged.Add(enemy))
                        {
                            float danoInfligido = GetEffectiveDamage();
                            enemy.TakeDamage(danoInfligido);
                            float curacion = vampiricoCuracion + danoInfligido * (vampiricoPorcentaje / 100f);
                            if (curacion > 0f && health != null)
                                health.Heal(curacion);
                        }
                    }
                }
            }

            if (attackTimer <= 0f && damageApplied)
                estaAtacando = false;
        }

        private void MostrarIndicadorAtaque(Vector3 center, float radius)
        {
            var indicator = new GameObject("AttackIndicator");
            indicator.transform.position = center;

            var lr = indicator.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 0f, 0f, 0.8f);
            lr.endColor = new Color(1f, 0f, 0f, 0.8f);
            lr.useWorldSpace = false;
            lr.sortingOrder = 100;
            lr.positionCount = 33;

            float angleStep = 360f / 32 * Mathf.Deg2Rad;
            for (int i = 0; i < 33; i++)
            {
                float angle = angleStep * i;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                lr.SetPosition(i, new Vector3(x, y, 0));
            }

            Destroy(indicator, 0.15f);
        }

        public void IncreaseDamage(float amount)
        {
            attackDamage += amount;
            danoBasePlano += amount;
        }

        public void SetMultiplicadorDano(float mult)
        {
            if (mult < 1f)
            {
                attackDamage *= mult;
                danoBasePlano *= mult;
            }
            else if (mult > multiplicadorDano)
            {
                multiplicadorDano = mult;
            }
            else
            {
                float flatBonus = (mult - 1f) * danoBasePlano;
                attackDamage += flatBonus;
                danoBasePlano += flatBonus;
            }
        }

        public void SetBonusRangoAtaque(float bonus)
        {
            bonusRangoAtaque += bonus;
        }

        public void SetBonusVelocidadAtaque(float bonus)
        {
            bonusVelocidadAtaque += bonus;
        }

        public void SetReduccionCooldownEscudo(float reduccion)
        {
            reduccionCooldownEscudo = reduccion;
        }

        public float GetAttackRadius()
        {
            return attackRadius + bonusRangoAtaque;
        }

        public float GetAttackDelay()
        {
            return attackDelay;
        }

        public float GetAttackCooldown()
        {
            return Mathf.Max(0.05f, attackCooldown - bonusVelocidadAtaque);
        }

        public float GetShieldCooldown()
        {
            return Mathf.Max(1f, shieldCooldown - reduccionCooldownEscudo);
        }

        public float GetEffectiveDamage()
        {
            float danoVelocista = velocistaActivo && movement != null ? movement.MoveSpeed : 0f;
            return attackDamage * multiplicadorDano + danoVelocista;
        }
    }
}