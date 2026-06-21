using UnityEngine;

namespace DragonCeltas
{
    public class ArqueroTorre : MonoBehaviour
    {
        [Header("Deteccion")]
        [SerializeField] private float rangoDeteccion = 8f;
        [SerializeField] private LayerMask capaEnemigos;

        [Header("Ataque")]
        [SerializeField] private float dano = 10f;
        [SerializeField] private float cadencia = 1.5f;
        [SerializeField] private float velocidadFlecha = 8f;
        [SerializeField] private float tiempoVidaFlecha = 3f;
        [SerializeField] private GameObject prefabFlecha;

        private float temporizadorDisparo;
        private Transform objetivoActual;

        void Update()
        {
            temporizadorDisparo -= Time.deltaTime;

            objetivoActual = BuscarEnemigoMasCercano();

            if (objetivoActual != null)
            {
                ApuntarAObjetivo();

                if (temporizadorDisparo <= 0f)
                {
                    Disparar();
                    temporizadorDisparo = cadencia;
                }
            }
        }

        private Transform BuscarEnemigoMasCercano()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion, capaEnemigos);
            Transform masCercano = null;
            float menorDistancia = Mathf.Infinity;

            foreach (var hit in hits)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < menorDistancia)
                {
                    menorDistancia = dist;
                    masCercano = hit.transform;
                }
            }

            return masCercano;
        }

        private void ApuntarAObjetivo()
        {
            Vector2 direccion = (objetivoActual.position - transform.position).normalized;

            if (Mathf.Abs(direccion.x) > 0.01f)
            {
                Vector3 escala = transform.localScale;
                escala.x = direccion.x > 0 ? 1f : -1f;
                transform.localScale = escala;
            }
        }

        private void Disparar()
        {
            Vector2 direccion = (objetivoActual.position - transform.position).normalized;

            if (prefabFlecha != null)
            {
                GameObject flecha = Instantiate(prefabFlecha, transform.position, Quaternion.identity);
                FlechaAliada flechaScript = flecha.GetComponent<FlechaAliada>();
                if (flechaScript == null)
                    flechaScript = flecha.AddComponent<FlechaAliada>();

                flechaScript.Inicializar(direccion, velocidadFlecha, dano, tiempoVidaFlecha, capaEnemigos);
            }
            else
            {
                CrearFlechaSimple(direccion);
            }
        }

        private void CrearFlechaSimple(Vector2 direccion)
        {
            GameObject flecha = new GameObject("FlechaAliada");
            flecha.transform.position = transform.position;

            var sr = flecha.AddComponent<SpriteRenderer>();
            sr.sprite = CrearSpriteFlecha();
            sr.color = new Color(0.3f, 0.8f, 1f);
            sr.sortingOrder = 50;

            var rb = flecha.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = direccion * velocidadFlecha;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = flecha.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;

            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            flecha.transform.rotation = Quaternion.Euler(0, 0, angulo);

            var flechaScript = flecha.AddComponent<FlechaAliada>();
            flechaScript.Inicializar(direccion, velocidadFlecha, dano, tiempoVidaFlecha, capaEnemigos);
        }

        private Sprite CrearSpriteFlecha()
        {
            int w = 32;
            int h = 8;
            var tex = new Texture2D(w, h);
            Color[] colores = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool esCuerpo = y >= 3 && y <= 4 && x < 24;
                    bool esPunta = x >= 20 && Mathf.Abs(y - 4) <= (x - 20) / 2f;
                    colores[y * w + x] = (esCuerpo || esPunta) ? Color.white : Color.clear;
                }
            }

            tex.SetPixels(colores);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 32f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        }
    }
}
