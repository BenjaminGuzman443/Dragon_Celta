using UnityEngine;
using UnityEngine.InputSystem;

namespace DragonCeltas
{
    public class LadronEnemy : MonoBehaviour, IEnemy
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float linearDrag = 5f;
        [SerializeField] private float mass = 10f;

        [Header("Ataque")]
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float pausaPostAtaque = 0.3f;

        [Header("Dash de Huida")]
        [SerializeField] private float dashFuerza = 15f;
        [SerializeField] private float duracionHuida = 5f;
        [SerializeField] private float velocidadHuida = 4f;

        [Header("Efectos Secundarios")]
        [SerializeField] private EfectosSecundariosConfig efectosSecundarios = new EfectosSecundariosConfig();

        [Header("Vida")]
        [SerializeField] private float maxHp = 25f;
        [SerializeField] private int xpAlMorir = 3;

        [Header("Dinero")]
        [SerializeField] private int oroMin = 8;
        [SerializeField] private int oroMax = 15;
        [SerializeField] private int oroRobablePorGolpe = 8;

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

        private bool enPausaPostAtaque;
        private float pausaPostAtaqueTimer;

        private float danoBase;
        private float maxHpBase;

        private bool huyendo;
        private float huyendoTimer;

        private bool estaAturdido;
        private float stunnTimer;

        private SpriteRenderer[] renderersCamuflaje;
        private bool camuflajeCacheInicializado;
        private float reveladoTimer;

        private int oroRobado;

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

            if (enPausaPostAtaque)
            {
                pausaPostAtaqueTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                if (pausaPostAtaqueTimer <= 0f)
                {
                    enPausaPostAtaque = false;
                    DashHaciaAtras();
                }
                animator.SetBool("isRunning", false);
                FlipSprite();
                ActualizarCamuflaje();
                return;
            }

            if (huyendo)
            {
                huyendoTimer -= Time.deltaTime;
                if (huyendoTimer <= 0f)
                    huyendo = false;
                else
                {
                    Vector2 dirHuida = ((Vector2)(transform.position - currentTarget.position)).normalized;
                    rb.linearVelocity = dirHuida * velocidadHuida;
                    animator.SetBool("isRunning", true);
                    FlipSprite();
                    ActualizarCamuflaje();
                    return;
                }
            }

            attackCooldownTimer -= Time.deltaTime;

            if (enRangoDeAtaque() && attackCooldownTimer <= 0f)
            {
                rb.linearVelocity = Vector2.zero;
                EjecutarAtaque();
            }
            else
            {
                Vector2 dir = ((Vector2)(currentTarget.position - transform.position)).normalized;
                rb.linearVelocity = dir * moveSpeed;
            }

            animator.SetBool("isRunning", rb.linearVelocity.magnitude > 0.1f);

            AplicarAvariciaStats();
            FlipSprite();
            ActualizarCamuflaje();
        }

        private void UpdateTarget()
        {
            if (DebugMode) return;

            if (playerTarget != null && playerHealth != null && !PlayerHealth.IsDead)
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

        private void EjecutarAtaque()
        {
            attackCooldownTimer = attackCooldown;

            animator.SetTrigger("attack");

            float dano = GetDanoConColera(attackDamage);

            if (currentCastleHealth != null)
            {
                currentCastleHealth.TakeDamage(dano);
            }
            else if (currentPlayerHealth != null)
            {
                currentPlayerHealth.TakeDamage(dano);

                if (efectosSecundarios.venenoActivo)
                    currentPlayerHealth.AplicarVeneno(efectosSecundarios.duracionVeneno);
                if (efectosSecundarios.camuflajeActivo)
                    reveladoTimer = 3f;

                if (playerUpgrades != null)
                {
                    int robado = Mathf.Min(oroRobablePorGolpe, playerUpgrades.OroDisponible);
                    if (robado > 0)
                    {
                        playerUpgrades.GastarOro(robado);
                        oroRobado += robado;
                    }
                }

                enPausaPostAtaque = true;
                pausaPostAtaqueTimer = pausaPostAtaque;
            }
        }

        private void DashHaciaAtras()
        {
            Vector2 direccionHuida = ((Vector2)(transform.position - currentTarget.position)).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direccionHuida * dashFuerza, ForceMode2D.Impulse);

            huyendo = true;
            huyendoTimer = duracionHuida;
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
            huyendo = false;
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
                int oroTotal = Random.Range(oroMin, oroMax + 1) + oroRobado / 2;
                GameEvents.GoldGained(oroTotal);
                GameEvents.EnemyDied();
                if (spawner != null)
                    spawner.HandleEnemyDeath();
                Destroy(gameObject);
            }
        }
    }
}
