using System.Collections;
using UnityEngine;

namespace DragonCeltas
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] private float maxHp = 100f;

        public static bool IsDead { get; private set; }
        public float MaxHp => maxHp;
        public float CurrentHp => hp;
        public float HpNormalized => Mathf.Clamp01(hp / maxHp);
        public bool EstaMuerto => isDead;
        public bool EstaEnvenenado => venenoDuracion > 0f;
        public bool EstaSobreCurado => hp > maxHpOriginal;
        public float RespawnTimer => respawnTimer;

        public float regeneracionPasiva;
        public float reduccionDelayRegeneracion;
        public float curacionFinRondaPorcentaje;
        public bool extasisActivo;

        private float hp;
        private bool isDead;
        private float maxHpOriginal;
        private float respawnTimer;
        private Vector3 spawnPosition;
        private float timerRegeneracion;

        private float venenoDuracion;
        private const float VENENO_MAX_DURACION = 10f;
        private const float VENENO_DANO_POR_SEGUNDO = 1.5f;
        private const float VENENO_POR_GOLPE = 2f;

        private SpriteRenderer spriteRenderer;
        private Collider2D col;
        private Rigidbody2D rb;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
            hp = maxHp;
            maxHpOriginal = maxHp;
            spawnPosition = transform.position;
            IsDead = false;
        }

        void Update()
        {
            if (isDead) return;

            if (venenoDuracion > 0f)
            {
                float danoVeneno = maxHp * (VENENO_DANO_POR_SEGUNDO / 100f) * Time.deltaTime;
                hp -= danoVeneno;
                venenoDuracion -= Time.deltaTime;
                if (venenoDuracion <= 0f)
                    venenoDuracion = 0f;
                if (hp <= 0f)
                {
                    hp = 0f;
                    isDead = true;
                    IsDead = true;
                    rb.linearVelocity = Vector2.zero;
                    spriteRenderer.enabled = false;
                    col.enabled = false;
                    venenoDuracion = 0f;
                    StartCoroutine(Respawnear());
                    return;
                }
            }

            if (regeneracionPasiva > 0f)
            {
                float delay = 5f - reduccionDelayRegeneracion;
                if (delay < 0.5f) delay = 0.5f;
                timerRegeneracion += Time.deltaTime;
                if (timerRegeneracion >= delay)
                {
                    timerRegeneracion = 0f;
                    Heal(regeneracionPasiva);
                }
            }

            if (hp > maxHpOriginal)
            {
                float decay = maxHpOriginal * 0.01f * Time.deltaTime;
                hp -= decay;
                if (hp < maxHpOriginal) hp = maxHpOriginal;
            }
        }

        public void SetMaxHpPorcentaje(float porcentaje)
        {
            maxHp += maxHp * (porcentaje / 100f);
        }

        public void SetMaxHp(float newMax)
        {
            maxHp = newMax;
        }

        public void Heal(float amount)
        {
            hp += amount;
            if (!extasisActivo && hp > maxHp) hp = maxHp;
        }

        public void AplicarEscudo(float multiplicador)
        {
            hp = maxHpOriginal * multiplicador;
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            hp -= damage;

            if (hp <= 0f)
            {
                hp = 0f;
                isDead = true;
                IsDead = true;
                rb.linearVelocity = Vector2.zero;
                spriteRenderer.enabled = false;
                col.enabled = false;
                StartCoroutine(Respawnear());
            }
        }

        public void AplicarVeneno(float duracion)
        {
            venenoDuracion += duracion;
            if (venenoDuracion > VENENO_MAX_DURACION)
                venenoDuracion = VENENO_MAX_DURACION;
        }

        private IEnumerator Respawnear()
        {
            respawnTimer = 10f;
            while (respawnTimer > 0f)
            {
                yield return new WaitForSeconds(1f);
                respawnTimer -= 1f;
            }
            hp = maxHp;
            transform.position = spawnPosition;
            spriteRenderer.enabled = true;
            col.enabled = true;
            isDead = false;
            IsDead = false;
        }

    }
}