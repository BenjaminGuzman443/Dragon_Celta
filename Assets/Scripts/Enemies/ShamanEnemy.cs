using UnityEngine;

namespace DragonCeltas
{
    public class ShamanEnemy : MonoBehaviour, IEnemy
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float playerAggroRange = 7f;
        [SerializeField] private float linearDrag = 5f;
        [SerializeField] private float mass = 10f;
        [SerializeField] private float distanciaIdeal = 5f;

        [Header("Ataque a Distancia")]
        [SerializeField] private float attackDamage = 6f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float velocidadProyectil = 6f;
        [SerializeField] private float tiempoVidaProyectil = 3f;
        [SerializeField] private float radioProyectil = 0.3f;
        [SerializeField] private Color colorProyectil = new Color(0.3f, 0.9f, 0.3f, 1f);
        [SerializeField] private GameObject prefabProyectil;

        [Header("Efectos Secundarios")]
        [SerializeField] private EfectosSecundariosConfig efectosSecundarios = new EfectosSecundariosConfig();

        [Header("Vida")]
        [SerializeField] private float maxHp = 40f;
        [SerializeField] private int xpAlMorir = 4;

        [Header("Dinero")]
        [SerializeField] private int oroMin = 5;
        [SerializeField] private int oroMax = 12;

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

        private bool estaAturdido;
        private float stunnTimer;

        private float danoBase;
        private float maxHpBase;

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
            if (estaAturdido)
            {
                stunnTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isRunning", false);
                if (stunnTimer <= 0f) estaAturdido = false;
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

            if (dist > distanciaIdeal * 1.2f)
            {
                Vector2 dirMov = ((Vector2)(currentTarget.position - transform.position)).normalized;
                rb.linearVelocity = dirMov * moveSpeed;
            }
            else if (dist < distanciaIdeal * 0.6f)
            {
                Vector2 dirMov = ((Vector2)(transform.position - currentTarget.position)).normalized;
                rb.linearVelocity = dirMov * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            attackCooldownTimer -= Time.deltaTime;

            if (attackCooldownTimer <= 0f && dist <= distanciaIdeal * 1.5f)
            {
                attackCooldownTimer = attackCooldown;
                DispararProyectil();
            }

            animator.SetBool("isRunning", rb.linearVelocity.magnitude > 0.1f);

            AplicarAvariciaStats();
            FlipSprite();
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

        private void DispararProyectil()
        {
            Vector2 direccion = ((Vector2)(currentTarget.position - transform.position)).normalized;
            float venenoDur = efectosSecundarios.venenoActivo ? efectosSecundarios.duracionVeneno : 0f;

            GameObject go;
            if (prefabProyectil != null)
            {
                go = Instantiate(prefabProyectil, transform.position, Quaternion.identity);
                var colPrefab = go.GetComponent<Collider2D>();
                if (colPrefab != null) colPrefab.isTrigger = true;
                var rbPrefab = go.GetComponent<Rigidbody2D>();
                if (rbPrefab == null) rbPrefab = go.AddComponent<Rigidbody2D>();
                rbPrefab.gravityScale = 0f;
                rbPrefab.linearVelocity = direccion * velocidadProyectil;
                rbPrefab.bodyType = RigidbodyType2D.Kinematic;
                var proyPrefab = go.GetComponent<ProyectilEnemigo>();
                if (proyPrefab == null) proyPrefab = go.AddComponent<ProyectilEnemigo>();
                proyPrefab.Inicializar(direccion, velocidadProyectil, GetDanoConColera(attackDamage), venenoDur, tiempoVidaProyectil);
            }
            else
            {
                go = new GameObject("PlasmaSphere");
                go.transform.position = transform.position;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = CrearCirculo(radioProyectil);
                sr.color = colorProyectil;
                sr.sortingOrder = 50;

                var col = go.AddComponent<CircleCollider2D>();
                col.radius = radioProyectil;
                col.isTrigger = true;

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearVelocity = direccion * velocidadProyectil;
                rb.bodyType = RigidbodyType2D.Kinematic;

                var proy = go.AddComponent<ProyectilEnemigo>();
                proy.Inicializar(direccion, velocidadProyectil, GetDanoConColera(attackDamage), venenoDur, tiempoVidaProyectil);
            }

            animator.SetTrigger("attack");
        }

        private Sprite CrearCirculo(float radius)
        {
            int size = 32;
            var tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float pixelRadius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist <= pixelRadius ? Color.white : Color.clear);
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2f));
        }

        private void FlipSprite()
        {
            if (currentTarget == null) return;
            Vector3 direction = currentTarget.position - transform.position;
            if (direction.x < -0.1f) spriteRenderer.flipX = true;
            if (direction.x > 0.1f) spriteRenderer.flipX = false;
        }

        public void Stun(float duration)
        {
            estaAturdido = true;
            stunnTimer = duration;
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
    }
}
