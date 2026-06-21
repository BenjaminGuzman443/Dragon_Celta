using UnityEngine;

namespace DragonCeltas
{
    public class FlechaAliada : MonoBehaviour
    {
        private float dano;
        private float tiempoVida;
        private float temporizadorVida;
        private LayerMask capaEnemigos;

        public void Inicializar(Vector2 direccion, float velocidad, float danoFlecha, float vida, LayerMask capa)
        {
            dano = danoFlecha;
            tiempoVida = vida;
            temporizadorVida = vida;
            capaEnemigos = capa;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            rb.linearVelocity = direccion * velocidad;

            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }

        void Update()
        {
            temporizadorVida -= Time.deltaTime;
            if (temporizadorVida <= 0f)
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & capaEnemigos) == 0) return;

            IEnemy enemigo = other.GetComponent<IEnemy>();
            if (enemigo != null)
            {
                enemigo.TakeDamage(dano);
                Destroy(gameObject);
            }
        }
    }
}
