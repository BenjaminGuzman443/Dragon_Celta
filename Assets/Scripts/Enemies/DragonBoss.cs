using UnityEngine;

namespace DragonCeltas
{
    public class DragonBoss : MonoBehaviour, IEnemy
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float playerAggroRange = 8f;
        [SerializeField] private float linearDrag = 5f;
        [SerializeField] private float mass = 20f;

        [Header("Ataque 1 - Arañaso")]
        [SerializeField] private float ataque1Damage = 15f;
        [SerializeField] private float ataque1Cooldown = 1.5f;
        [SerializeField] private float ataque1Rango = 3f;
        [SerializeField] private float ataque1Delay = 0.3f;

        [Header("Ataque 2 - Aliento de Fuego")]
        [SerializeField] private float ataque2Damage = 10f;
        [SerializeField] private float ataque2Cooldown = 3f;
        [SerializeField] private float ataque2Rango = 6f;
        [SerializeField] private float ataque2Delay = 0.5f;
        [SerializeField] private float zonaFuegoDuracion = 4f;
        [SerializeField] private float zonaFuegoTick = 0.5f;
        [SerializeField] private GameObject prefabFuegoSuelo;

        [Header("Aturdimiento")]
        [SerializeField] private float duracionStun = 2f;

        [Header("Efectos Secundarios")]
        [SerializeField] private EfectosSecundariosConfig efectosSecundarios = new EfectosSecundariosConfig();

        [Header("Vida")]
        [SerializeField] private float maxHp = 300f;
        [SerializeField] private int xpAlMorir = 50;

        [Header("Dinero")]
        [SerializeField] private int oroMin = 50;
        [SerializeField] private int oroMax = 100;

        [HideInInspector] public Spawner spawner;

        [Header("Referencias")]
        [SerializeField] private GameObject castle;
        [SerializeField] private GameObject player;

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
        private float ataque1CooldownTimer;
        private float ataque2CooldownTimer;

        private bool estaAturdido;
        private float stunnTimer;
        private int stunsRecientes;
        private float stunCooldownTimer;

        private float danoBaseAtaque1;
        private float danoBaseAtaque2;
        private float maxHpBase;

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

            danoBaseAtaque1 = ataque1Damage;
            danoBaseAtaque2 = ataque2Damage;
            maxHpBase = maxHp;
            hp = maxHp;
        }

        public void EscalarStats(int ronda)
        {
            hp = maxHp;
            if (healthBar != null)
                healthBar.SetHealth(hp, maxHp);
        }

        void Update()
        {
            if (hp <= 0f) return;

            if (stunCooldownTimer > 0f)
            {
                stunCooldownTimer -= Time.deltaTime;
                if (stunCooldownTimer <= 0f)
                    stunsRecientes = 0;
            }

            if (estaAturdido)
            {
                stunnTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("IsRunning", false);
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

            float dist = Vector2.Distance(transform.position, currentTarget.position);

            if (dist <= ataque1Rango && ataque1CooldownTimer <= 0f)
            {
                Ataque1();
            }
            else if (dist <= ataque2Rango && ataque2CooldownTimer <= 0f)
            {
                Ataque2();
            }
            else if (dist <= ataque1Rango || dist <= ataque2Rango)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("IsRunning", false);
            }
            else
            {
                Vector2 dirMov = ((Vector2)(currentTarget.position - transform.position)).normalized;
                rb.linearVelocity = dirMov * moveSpeed;
                animator.SetBool("IsRunning", true);
            }

            ataque1CooldownTimer -= Time.deltaTime;
            ataque2CooldownTimer -= Time.deltaTime;

            AplicarAvariciaStats();
            FlipSprite();
            ActualizarCamuflaje();
        }

        private void UpdateTarget()
        {
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

        private void Ataque1()
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
            ataque1CooldownTimer = ataque1Cooldown;
            animator.SetTrigger("Attack1");
            StartCoroutine(DanoConDelay(ataque1Delay));
        }

        private System.Collections.IEnumerator DanoConDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (currentCastleHealth != null)
                currentCastleHealth.TakeDamage(ataque1Damage);
            else if (currentPlayerHealth != null)
            {
                currentPlayerHealth.TakeDamage(ataque1Damage);
                if (efectosSecundarios.venenoActivo)
                    currentPlayerHealth.AplicarVeneno(efectosSecundarios.duracionVeneno);
                if (efectosSecundarios.camuflajeActivo)
                    reveladoTimer = 3f;
            }
        }

        private void Ataque2()
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
            ataque2CooldownTimer = ataque2Cooldown;
            animator.SetTrigger("Attack2");
            StartCoroutine(AlientoFuegoConDelay());
        }

        private System.Collections.IEnumerator AlientoFuegoConDelay()
        {
            yield return new WaitForSeconds(ataque2Delay);
            AlientoFuego();
        }

        private void AlientoFuego()
        {
            Vector3 posicion = currentTarget != null ? currentTarget.position : transform.position;

            var zonaGO = new GameObject("ZonaFuego");
            zonaGO.transform.position = posicion;
            var zona = zonaGO.AddComponent<ZonaFuego>();
            zona.Inicializar(ataque2Rango, ataque2Damage, zonaFuegoTick, zonaFuegoDuracion,
                efectosSecundarios.venenoActivo, efectosSecundarios.duracionVeneno, prefabFuegoSuelo);
        }

        private void FlipSprite()
        {
            if (currentTarget == null) return;
            Vector3 direction = currentTarget.position - transform.position;
            if (direction.x < -0.1f) spriteRenderer.flipX = false;
            if (direction.x > 0.1f) spriteRenderer.flipX = true;
        }

        private void AplicarAvariciaStats()
        {
            if (!efectosSecundarios.avariciaActivo || playerUpgrades == null) return;

            int oro = playerUpgrades.OroDisponible;
            int bonusDano = (oro / 10) * 2;
            float bonusVida = (oro / 10) * 10f;

            ataque1Damage = danoBaseAtaque1 + bonusDano;
            ataque2Damage = danoBaseAtaque2 + bonusDano;
            maxHp = maxHpBase + bonusVida;
            if (hp > maxHp) hp = maxHp;
        }

        private float GetDanoConColera(float dano)
        {
            if (!efectosSecundarios.coleraActivo) return dano;
            if (hp <= maxHp * 0.5f) return dano * 1.3f;
            return dano;
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

        public void TakeDamage(float damage)
        {
            hp -= damage;
            if (healthBar != null)
                healthBar.SetHealth(hp, maxHp);

            if (hp > 0f)
            {
                if (stunCooldownTimer <= 0f)
                    stunsRecientes = 0;

                if (stunsRecientes < 2)
                {
                    animator.SetTrigger("Hit");
                    estaAturdido = true;
                    stunnTimer = duracionStun;
                    stunsRecientes++;
                    if (stunCooldownTimer <= 0f)
                        stunCooldownTimer = 5f;
                }
            }

            if (hp <= 0f)
            {
                hp = 0f;
                animator.SetBool("IsDead", true);
                animator.SetTrigger("Hit");
                rb.linearVelocity = Vector2.zero;
                GameEvents.ScoreGained(xpAlMorir);
                GameEvents.GoldGained(Random.Range(oroMin, oroMax + 1));
                GameEvents.EnemyDied();
                if (spawner != null)
                    spawner.HandleEnemyDeath();
                Destroy(gameObject, 2f);
                enabled = false;
            }
        }
    }
}
