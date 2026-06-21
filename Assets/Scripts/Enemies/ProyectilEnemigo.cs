using UnityEngine;

namespace DragonCeltas
{
    public class ProyectilEnemigo : MonoBehaviour
    {
        private float velocidad;
        private float dano;
        private float duracionVeneno;
        private float tiempoVida;
        private float timerVida;
        private Vector2 direccion;
        private Rigidbody2D rb;
        private GameObject prefabEfectoImpacto;

        public void Inicializar(Vector2 dir, float vel, float dmg, float venenoDur, float vida)
        {
            inicializarInterno(dir, vel, dmg, venenoDur, vida, null);
        }

        public void InicializarConEfecto(Vector2 dir, float vel, float dmg, float venenoDur, float vida, GameObject efectoImpacto)
        {
            inicializarInterno(dir, vel, dmg, venenoDur, vida, efectoImpacto);
        }

        private void inicializarInterno(Vector2 dir, float vel, float dmg, float venenoDur, float vida, GameObject efecto)
        {
            direccion = dir.normalized;
            velocidad = vel;
            dano = dmg;
            duracionVeneno = venenoDur;
            tiempoVida = vida;
            timerVida = vida;
            prefabEfectoImpacto = efecto;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            if (rb != null)
                rb.linearVelocity = direccion * velocidad;

            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        }

        void Update()
        {
            timerVida -= Time.deltaTime;
            if (timerVida <= 0f)
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var castle = other.GetComponentInParent<CastleHealth>();
            if (castle != null)
            {
                castle.TakeDamage(dano);
                SpawnearEfecto();
                Destroy(gameObject);
                return;
            }

            var player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(dano);
                if (duracionVeneno > 0f)
                    player.AplicarVeneno(duracionVeneno);
                SpawnearEfecto();
                Destroy(gameObject);
            }
        }

        private void SpawnearEfecto()
        {
            if (prefabEfectoImpacto != null)
                Instantiate(prefabEfectoImpacto, transform.position, Quaternion.identity);
        }
    }
}
