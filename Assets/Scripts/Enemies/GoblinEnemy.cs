using System.Collections.Generic;
using UnityEngine;

namespace DragonCeltas
{
    public class GoblinEnemy : MonoBehaviour, IEnemy
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float playerAggroRange = 5f;
        [SerializeField] private float linearDrag = 5f;
        [SerializeField] private float mass = 10f;

        [Header("Combo de Ataque")]
        [SerializeField] private float ataque1Damage = 10f;
        [SerializeField] private float ataque1Radius = 1.5f;
        [SerializeField] private float ataque2Damage = 15f;
        [SerializeField] private Vector2 ataque2Size = new Vector2(3f, 1.5f);
        [SerializeField] private float delayEntreAtaques = 0.4f;
        [SerializeField] private float cooldownCombo = 2f;

        [Header("Efectos Secundarios")]
        [SerializeField] private EfectosSecundariosConfig efectosSecundarios = new EfectosSecundariosConfig();

        [Header("Vida")]
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private int xpAlMorir = 5;

        [Header("Dinero")]
        [SerializeField] private int oroMin = 5;
        [SerializeField] private int oroMax = 10;

        [Header("Referencias")]
        [SerializeField] private GameObject castle;
        [SerializeField] private GameObject player;

        [HideInInspector] public Spawner spawner;

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
        private float comboTimer;
        private float comboCooldownTimer;
        private bool enCombo;
        private bool fase1Hecha;
        private bool fase2Hecha;

        private float danoBaseAtaque1;
        private float danoBaseAtaque2;
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
        }

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            healthBar = GetComponent<HealthBar>();

            if (castle == null) castle = GameObject.Find("Castle");
            if (player == null) player = GameObject.Find("JulianWarrior");

            if (castle != null) { castleTarget = castle.transform; castleHealth = castle.GetComponent<CastleHealth>(); }
            if (player != null) { playerTarget = player.transform; playerHealth = player.GetComponent<PlayerHealth>(); playerUpgrades = player.GetComponent<PlayerUpgrades>(); }

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
            if (healthBar != null) healthBar.SetHealth(hp, maxHp);
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

        void Update()
        {
            if (estaAturdido)
            {
                stunnTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                if (stunnTimer <= 0f) estaAturdido = false;
                return;
            }

            if (castleTarget == null || castleHealth == null) { rb.linearVelocity = Vector2.zero; return; }

            UpdateTarget();

            if (enCombo)
            {
                comboTimer += Time.deltaTime;

                if (!fase1Hecha)
                {
                    fase1Hecha = true;
                    MostrarIndicadorCirculo(transform.position, ataque1Radius);
                    AplicarAtaqueCircular();
                }

                if (!fase2Hecha && comboTimer >= delayEntreAtaques)
                {
                    fase2Hecha = true;
                    Vector2 dir = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
                    Vector2 center = (Vector2)transform.position + dir * (ataque2Size.x * 0.5f);
                    MostrarIndicadorRectangulo(center, ataque2Size);
                    AplicarAtaqueRecto(dir, center);
                }

                if (comboTimer >= delayEntreAtaques + 0.3f)
                {
                    enCombo = false;
                    comboCooldownTimer = cooldownCombo;
                }
            }
            else
            {
                comboCooldownTimer -= Time.deltaTime;

                if (comboCooldownTimer <= 0f && EnRangoDeAtaque())
                {
                    IniciarCombo();
                }
                else
                {
                    Vector2 dir = ((Vector2)(currentTarget.position - transform.position)).normalized;
                    rb.linearVelocity = dir * moveSpeed;
                }
            }

            if (spriteRenderer != null && currentTarget != null)
            {
                Vector3 d = currentTarget.position - transform.position;
                if (d.x < -0.1f) spriteRenderer.flipX = true;
                if (d.x > 0.1f) spriteRenderer.flipX = false;
            }

            if (animator != null)
                animator.SetBool("isRunning", rb.linearVelocity.magnitude > 0.1f);

            AplicarAvariciaStats();
            ActualizarCamuflaje();
        }

        private void UpdateTarget()
        {
            if (playerTarget != null && playerHealth != null && !PlayerHealth.IsDead)
            {
                float distToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                float distToCastle = castleTarget != null ? Vector2.Distance(transform.position, castleTarget.position) : float.MaxValue;

                if (distToPlayer <= playerAggroRange && distToPlayer < distToCastle)
                {
                    currentTarget = playerTarget; currentCastleHealth = null; currentPlayerHealth = playerHealth;
                    return;
                }
            }
            currentTarget = castleTarget; currentCastleHealth = castleHealth; currentPlayerHealth = null;
        }

        private void IniciarCombo()
        {
            enCombo = true;
            comboTimer = 0f;
            fase1Hecha = false;
            fase2Hecha = false;
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetTrigger("attack");
        }

        private bool EnRangoDeAtaque()
        {
            if (currentTarget == null) return false;
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            float descuento = 0f;

            var cols = currentTarget.GetComponentsInChildren<Collider2D>();
            foreach (var col in cols)
            {
                float ext = col.bounds.extents.magnitude * 0.5f;
                if (ext > descuento) descuento = ext;
            }

            return (dist - descuento) <= ataque1Radius * 1.5f;
        }

        private void AplicarAtaqueCircular()
        {
            float d1 = GetDanoConColera(ataque1Damage);
            if (currentCastleHealth != null)
                currentCastleHealth.TakeDamage(d1);
            else if (currentPlayerHealth != null)
            {
                currentPlayerHealth.TakeDamage(d1);
                if (efectosSecundarios.venenoActivo)
                        currentPlayerHealth.AplicarVeneno(efectosSecundarios.duracionVeneno);
                if (efectosSecundarios.camuflajeActivo)
                    reveladoTimer = 3f;
            }
            MostrarIndicadorCirculo(transform.position, ataque1Radius);
        }

        private void AplicarAtaqueRecto(Vector2 dir, Vector2 center)
        {
            float d2 = GetDanoConColera(ataque2Damage);
            if (currentCastleHealth != null)
                currentCastleHealth.TakeDamage(d2);
            else if (currentPlayerHealth != null)
            {
                currentPlayerHealth.TakeDamage(d2);
                if (efectosSecundarios.venenoActivo)
                        currentPlayerHealth.AplicarVeneno(efectosSecundarios.duracionVeneno);
                if (efectosSecundarios.camuflajeActivo)
                    reveladoTimer = 3f;
            }
            MostrarIndicadorRectangulo(center, ataque2Size);
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

        public void Stun(float duration) { estaAturdido = true; stunnTimer = duration; enCombo = false; }
        public void ApplyKnockback(Vector2 dir, float force) { rb.linearVelocity = Vector2.zero; rb.AddForce(dir * force, ForceMode2D.Impulse); }

        public void RevelarCamuflaje()
        {
            reveladoTimer = 3f;
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

        public void TakeDamage(float damage)
        {
            hp -= damage;
            if (healthBar != null) healthBar.SetHealth(hp, maxHp);
            if (hp <= 0f)
            {
                GameEvents.ScoreGained(xpAlMorir);
                GameEvents.GoldGained(Random.Range(oroMin, oroMax + 1));
                GameEvents.EnemyDied();
                if (spawner != null) spawner.HandleEnemyDeath();
                Destroy(gameObject);
            }
        }

        private void MostrarIndicadorCirculo(Vector3 center, float radius)
        {
            var go = new GameObject("AtkCircle");
            go.transform.position = center;
            var lr = go.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 0f, 0f, 0.7f);
            lr.endColor = new Color(1f, 0f, 0f, 0.7f);
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 33;
            for (int i = 0; i < 33; i++)
            {
                float a = i * Mathf.PI * 2f / 32f;
                lr.SetPosition(i, center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0));
            }
            Destroy(go, 0.3f);
        }

        private void MostrarIndicadorRectangulo(Vector3 center, Vector2 size)
        {
            var go = new GameObject("AtkBox");
            go.transform.position = center;
            var lr = go.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 0.5f, 0f, 0.7f);
            lr.endColor = new Color(1f, 0.5f, 0f, 0.7f);
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 5;
            float hw = size.x * 0.5f;
            float hh = size.y * 0.5f;
            lr.SetPosition(0, center + new Vector3(-hw, -hh, 0));
            lr.SetPosition(1, center + new Vector3(-hw, hh, 0));
            lr.SetPosition(2, center + new Vector3(hw, hh, 0));
            lr.SetPosition(3, center + new Vector3(hw, -hh, 0));
            lr.SetPosition(4, center + new Vector3(-hw, -hh, 0));
            Destroy(go, 0.3f);
        }
    }
}
