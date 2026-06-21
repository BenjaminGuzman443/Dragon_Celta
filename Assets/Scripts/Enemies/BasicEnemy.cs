using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragonCeltas
{
    public class BasicEnemy : MonoBehaviour, IEnemy
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float playerAggroRange = 5f;
        [SerializeField] private float linearDrag = 5f;
        [SerializeField] private float mass = 10f;

        [Header("Ataque")]
        [SerializeField] private float attackDuration = 0.4f;
        [SerializeField] private float attackDamage = 5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float postAttackFreeze = 0.3f;
        [SerializeField] private float attackRange = 2.5f;

        [Header("Efectos Secundarios")]
        [SerializeField] private EfectosSecundariosConfig efectosSecundarios = new EfectosSecundariosConfig();

        [Header("Vida")]
        [SerializeField] private float maxHp = 35f;
        [SerializeField] private int xpAlMorir = 1;

        [Header("Dinero")]
        [SerializeField] private int oroMin = 1;
        [SerializeField] private int oroMax = 3;

        [HideInInspector] public Spawner spawner;

        [Header("Referencias")]
        [SerializeField] private GameObject castle;
        [SerializeField] private GameObject player;

        public static bool DebugMode { get; private set; }
        private bool debugKeyWasHeld;

        private Transform castleTarget;
        private CastleHealth castleHealth;
        private Transform playerTarget;
        private PlayerHealth playerHealth;
        private PlayerUpgrades playerUpgrades;

        private Transform currentTarget;
        private CastleHealth currentCastleHealth;
        private PlayerHealth currentPlayerHealth;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private HealthBar healthBar;

        private float hp;
        private float attackCooldownTimer;
        private float attackDurationTimer;
        private float postAttackFreezeTimer;
        private bool estaAtacando;
        private bool inPostAttackFreeze;

        private float danoBase;
        private float maxHpBase;

        private bool estaAturdido;
        private float stunnTimer;

        private SpriteRenderer[] renderersCamuflaje;
        private bool camuflajeCacheInicializado;
        private float reveladoTimer;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.linearDamping = linearDrag;
            rb.mass = mass;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var triggerCol = GetComponent<CircleCollider2D>();
            if (triggerCol != null)
            {
                var solidCol = gameObject.AddComponent<CircleCollider2D>();
                solidCol.radius = triggerCol.radius;
                solidCol.isTrigger = false;

                var physMat = new PhysicsMaterial2D("EnemySlide");
                physMat.friction = 0f;
                physMat.bounciness = 0f;
                solidCol.sharedMaterial = physMat;
            }

            animator = GetComponent<Animator>();
        }

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            healthBar = GetComponent<HealthBar>();

            if (castle == null)
                castle = GameObject.Find("Castle");
            if (player == null)
                player = GameObject.Find("JulianWarrior");

            if (castle != null)
            {
                castleTarget = castle.transform;
                castleHealth = castle.GetComponent<CastleHealth>();
            }

            if (player != null)
            {
                playerTarget = player.transform;
                playerHealth = player.GetComponent<PlayerHealth>();
                playerUpgrades = player.GetComponent<PlayerUpgrades>();
            }

            currentTarget = castleTarget;
            currentCastleHealth = castleHealth;

            danoBase = attackDamage;
            maxHpBase = maxHp;
            hp = maxHp;
        }

        public void EscalarStats(int ronda)
        {
            hp = maxHp;

            if (healthBar != null)
                healthBar.SetHealth(hp, maxHp);
        }

        private void AplicarAvariciaStats()
        {
            if (!efectosSecundarios.avariciaActivo || playerUpgrades == null) return;

            int oro = playerUpgrades.OroDisponible;
            attackDamage = danoBase + (oro / 10) * 2;
            maxHp = maxHpBase + (oro / 10) * 10f;
            if (hp > maxHp) hp = maxHp;
        }

        private float GetDanoConColera(float dano)
        {
            if (!efectosSecundarios.coleraActivo) return dano;
            if (hp <= maxHp * 0.5f) return dano * 1.3f;
            return dano;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.lKey.isPressed)
                {
                    if (!debugKeyWasHeld)
                        DebugMode = !DebugMode;
                    debugKeyWasHeld = true;
                }
                else
                {
                    debugKeyWasHeld = false;
                }
            }

            if (estaAturdido)
            {
                stunnTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isRunning", false);
                if (stunnTimer <= 0f)
                    estaAturdido = false;
                return;
            }

            if (castleTarget == null || castleHealth == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            UpdateTarget();

            if (currentTarget == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (estaAtacando)
            {
                attackDurationTimer -= Time.deltaTime;
                if (attackDurationTimer <= 0f)
                {
                    estaAtacando = false;
                    inPostAttackFreeze = true;
                    postAttackFreezeTimer = postAttackFreeze;
                }
            }
            else if (inPostAttackFreeze)
            {
                postAttackFreezeTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                if (postAttackFreezeTimer <= 0f)
                    inPostAttackFreeze = false;
            }
            else if (enRangoDeAtaque() && attackCooldownTimer <= 0f)
            {
                rb.linearVelocity = Vector2.zero;
                StartAttack();
            }
            else
            {
                Vector2 dir = ((Vector2)(currentTarget.position - transform.position)).normalized;
                Vector2 velocity = dir * moveSpeed;

                var nearby = Physics2D.OverlapCircleAll(transform.position, 0.8f);
                foreach (var c in nearby)
                {
                    if (c.gameObject == gameObject) continue;
                    BasicEnemy other = c.GetComponent<BasicEnemy>();
                    if (other != null)
                    {
                        Vector2 away = (Vector2)(transform.position - c.transform.position);
                        float dist = away.magnitude;
                        if (dist > 0.001f && dist < 0.8f)
                            velocity += away.normalized * (1f - dist / 0.8f) * moveSpeed;
                    }
                }

                rb.linearVelocity = velocity;
            }

            if (!estaAtacando)
                attackCooldownTimer -= Time.deltaTime;

            animator.SetBool("isRunning", rb.linearVelocity.magnitude > 0.1f);

            AplicarAvariciaStats();
            FlipSprite();
            ActualizarCamuflaje();
        }

        private void UpdateTarget()
        {
            if (DebugMode) return;

            float distToCastle = castleTarget != null ? Vector2.Distance(transform.position, castleTarget.position) : float.MaxValue;
            float distToPlayer = float.MaxValue;

            if (playerTarget != null && playerHealth != null && !PlayerHealth.IsDead)
                distToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distToPlayer <= playerAggroRange && distToPlayer < distToCastle)
            {
                currentTarget = playerTarget;
                currentCastleHealth = null;
                currentPlayerHealth = playerHealth;
            }
            else
            {
                currentTarget = castleTarget;
                currentCastleHealth = castleHealth;
                currentPlayerHealth = null;
            }
        }

        private bool enRangoDeAtaque()
        {
            if (currentTarget == null) return false;
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            float descuento = 0f;

            var cols = currentTarget.GetComponentsInChildren<Collider2D>();
            foreach (var col in cols)
            {
                float ext = col.bounds.extents.magnitude * 0.5f;
                if (ext > descuento)
                    descuento = ext;
            }

            return (dist - descuento) <= attackRange;
        }

        private void StartAttack()
        {
            estaAtacando = true;
            attackDurationTimer = attackDuration;
            attackCooldownTimer = attackCooldown;

            animator.SetTrigger("attack");

            float dano = GetDanoConColera(attackDamage);

            if (currentCastleHealth != null)
                currentCastleHealth.TakeDamage(dano);
            else if (currentPlayerHealth != null)
            {
                currentPlayerHealth.TakeDamage(dano);
                if (efectosSecundarios.venenoActivo)
                    currentPlayerHealth.AplicarVeneno(efectosSecundarios.duracionVeneno);
                if (efectosSecundarios.camuflajeActivo)
                    reveladoTimer = 3f;
            }
        }

        private void FlipSprite()
        {
            if (currentTarget == null) return;
            Vector3 direction = currentTarget.position - transform.position;
            if (direction.x < -0.1f) spriteRenderer.flipX = true;
            if (direction.x > 0.1f) spriteRenderer.flipX = false;
        }

        private void ActualizarCamuflaje()
        {
            if (!efectosSecundarios.camuflajeActivo) return;

            if (!camuflajeCacheInicializado)
            {
                renderersCamuflaje = GetComponentsInChildren<SpriteRenderer>();
                camuflajeCacheInicializado = true;
            }

            if (reveladoTimer > 0f)
                reveladoTimer -= Time.deltaTime;

            float alpha;
            if (reveladoTimer > 0f)
                alpha = 1f;
            else
            {
                float dist = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : 0f;
                alpha = Mathf.Lerp(1f, efectosSecundarios.camuflajeTransparencia, Mathf.Clamp01(dist / efectosSecundarios.camuflajeRango));
            }

            foreach (var sr in renderersCamuflaje)
            {
                if (sr == null) continue;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        public void Stun(float duration)
        {
            estaAturdido = true;
            stunnTimer = duration;
            estaAtacando = false;
            inPostAttackFreeze = false;
        }

        public void RevelarCamuflaje()
        {
            reveladoTimer = 3f;
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }

        public void ImpulsoInicial(float extraVelocidad, float duracion)
        {
            StartCoroutine(AplicarImpulso(extraVelocidad, duracion));
        }

        private System.Collections.IEnumerator AplicarImpulso(float extra, float duracion)
        {
            moveSpeed += extra;
            yield return new WaitForSeconds(duracion);
            moveSpeed -= extra;
        }

        public void TakeDamage(float damage)
        {
            hp -= damage;
            if (healthBar != null)
                healthBar.SetHealth(hp, maxHp);
            if (hp <= 0f)
            {
                GameEvents.ScoreGained(xpAlMorir);
                GameEvents.GoldGained(Random.Range(oroMin, oroMax + 1));
                GameEvents.EnemyDied();
                if (spawner != null)
                    spawner.HandleEnemyDeath();
                Destroy(gameObject);
            }
        }

        void OnGUI() { }
    }
}